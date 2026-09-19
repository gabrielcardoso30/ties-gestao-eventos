using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
    TimeProvider timeProvider) : IUseCase<CriarSessaoRequest, CriarSessaoResponse>
{
    public async Task<Result<CriarSessaoResponse>> HandleAsync(CriarSessaoRequest request, CancellationToken cancellationToken)
    {
        var emailNormalizado = userManager.NormalizeEmail(request.UsuarioEmail.Trim());
        var usuario = await db.Usuarios
            .TagWith("Identidade.CriarSessao.CarregarUsuario")
            .FirstOrDefaultAsync(u => u.NormalizedEmail == emailNormalizado, cancellationToken);
        if (usuario is null)
        {
            return IdentidadeErros.CredenciaisInvalidas;
        }

        if (!usuario.EstaAtivo)
        {
            return IdentidadeErros.UsuarioInativo;
        }

        // lockoutOnFailure: cada falha incrementa AccessFailedCount (persistido pelo Identity); na 5ª o usuário é bloqueado por 5 min.
        var verificacao = await signInManager.CheckPasswordSignInAsync(usuario, request.Senha, lockoutOnFailure: true);
        if (verificacao.IsLockedOut)
        {
            return IdentidadeErros.UsuarioBloqueado;
        }

        if (!verificacao.Succeeded)
        {
            return IdentidadeErros.CredenciaisInvalidas;
        }

        var perfis = await db.UsuarioPerfis
            .TagWith("Identidade.CriarSessao.CarregarPerfis")
            .AsNoTracking()
            .Where(up => up.UserId == usuario.Id)
            .Join(db.Perfis, up => up.RoleId, p => p.Id, (up, p) => p.Name!)
            .OrderBy(nome => nome)
            .ToListAsync(cancellationToken);

        usuario.RegistrarAcesso(timeProvider.GetUtcNow());

        return await db.ExecuteInTransactionAsync(async ct =>
        {
            await db.SaveChangesAsync(ct);
            var token = tokenService.Gerar(usuario, perfis);
            return Result.Success(new CriarSessaoResponse(
                token.AccessToken,
                token.ExpiraEm,
                new CriarSessaoUsuarioResponse(usuario.Id, usuario.UsuarioNome, usuario.Email ?? string.Empty, perfis)));
        }, cancellationToken);
    }
}
