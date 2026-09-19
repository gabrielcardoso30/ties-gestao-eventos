using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Module.Identidade.Domain;
using Module.Identidade.Shared;
using Module.Identidade.Shared.Seguranca;
using Shared.Data.Extensions;
using Shared.Http.Endpoints;
using Shared.Http.Results;

namespace Module.Identidade.UseCases.CriarSessao;

/// <summary>
/// Login por e-mail/senha. A verificação de senha e o lockout ficam com o <see cref="SignInManager{TUser}"/> (sem cookies);
/// o sucesso atualiza <c>UltimoAcessoEm</c> e grava <c>UsuarioAutenticado</c> no Outbox na mesma transação.
/// </summary>
internal sealed class CriarSessaoUseCase(
    IdentidadeDbContext db,
    UserManager<Usuario> userManager,
    SignInManager<Usuario> signInManager,
    ITokenService tokenService,
    TimeProvider timeProvider,
    ILogger<CriarSessaoUseCase> logger) : IUseCase<CriarSessaoRequest, CriarSessaoResponse>
{
    public async Task<Result<CriarSessaoResponse>> HandleAsync(CriarSessaoRequest request, CancellationToken cancellationToken)
    {
        var emailNormalizado = userManager.NormalizeEmail(request.UsuarioEmail.Trim());
        var usuario = await db.Usuarios
            .TagWith("Identidade.CriarSessao.CarregarUsuario")
            .FirstOrDefaultAsync(u => u.NormalizedEmail == emailNormalizado, cancellationToken);
        if (usuario is null)
        {
            logger.LogInformation("Autenticação rejeitada: credencial não corresponde a usuário cadastrado");
            return IdentidadeErros.CredenciaisInvalidas;
        }

        if (!usuario.EstaAtivo)
        {
            logger.LogInformation("Autenticação rejeitada: usuário {UsuarioId} está inativo", usuario.Id);
            return IdentidadeErros.UsuarioInativo;
        }

        // lockoutOnFailure: cada falha incrementa AccessFailedCount (persistido pelo Identity); na 5ª o usuário é bloqueado por 5 min.
        logger.LogDebug("Validando senha do usuário {UsuarioId} com política de bloqueio habilitada", usuario.Id);
        var verificacao = await signInManager.CheckPasswordSignInAsync(usuario, request.Senha, lockoutOnFailure: true);
        if (verificacao.IsLockedOut)
        {
            logger.LogWarning("Autenticação bloqueada para usuário {UsuarioId} após falhas consecutivas", usuario.Id);
            return IdentidadeErros.UsuarioBloqueado;
        }

        if (!verificacao.Succeeded)
        {
            logger.LogInformation("Autenticação rejeitada por credencial inválida para usuário {UsuarioId}; tentativas falhas={FailedAccessCount}", usuario.Id, usuario.AccessFailedCount);
            return IdentidadeErros.CredenciaisInvalidas;
        }

        var perfis = await db.UsuarioPerfis
            .TagWith("Identidade.CriarSessao.CarregarPerfis")
            .AsNoTracking()
            .Where(up => up.UserId == usuario.Id)
            .Join(db.Perfis, up => up.RoleId, p => p.Id, (up, p) => p.Name!)
            .OrderBy(nome => nome)
            .ToListAsync(cancellationToken);
        logger.LogInformation("Usuário {UsuarioId} autenticado com {ProfileCount} perfil(is)", usuario.Id, perfis.Count);

        logger.LogDebug("Chamando agregado Usuário {UsuarioId}.RegistrarAcesso para registrar autenticação e evento de integração", usuario.Id);
        usuario.RegistrarAcesso(timeProvider.GetUtcNow());

        return await db.ExecuteInTransactionAsync(async ct =>
        {
            await db.SaveChangesAsync(ct);
            logger.LogDebug("Último acesso e evento de autenticação persistidos para usuário {UsuarioId}; gerando token", usuario.Id);
            var token = tokenService.Gerar(usuario, perfis);
            return Result.Success(new CriarSessaoResponse(
                token.AccessToken,
                token.ExpiraEm,
                new CriarSessaoUsuarioResponse(usuario.Id, usuario.UsuarioNome, usuario.Email ?? string.Empty, perfis)));
        }, cancellationToken);
    }
}
