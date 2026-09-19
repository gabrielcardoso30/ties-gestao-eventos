using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Module.Identidade.Domain;
using Module.Identidade.Shared;
using Shared.Data.Extensions;
using Shared.Http.Endpoints;
using Shared.Http.Results;

namespace Module.Identidade.UseCases.RegistrarUsuario;

/// <summary>
/// Registro de usuário por um administrador. <c>UserManager.CreateAsync</c> valida e-mail/senha e persiste (SaveChanges próprio),
/// por isso o evento <c>UsuarioRegistrado</c> é registrado no agregado ANTES, em <see cref="Usuario.Criar"/>, e cai no Outbox na mesma transação.
/// </summary>
internal sealed class RegistrarUsuarioUseCase(IdentidadeDbContext db, UserManager<Usuario> userManager, ILogger<RegistrarUsuarioUseCase> logger) : IUseCase<RegistrarUsuarioRequest, RegistrarUsuarioResponse>
{
    public async Task<Result<RegistrarUsuarioResponse>> HandleAsync(RegistrarUsuarioRequest request, CancellationToken cancellationToken)
    {
        logger.LogDebug("Chamando regra de domínio PerfisValidos.Normalizar para {ProfileCount} perfil(is) solicitado(s)", request.Perfis.Count);
        var perfis = PerfisValidos.Normalizar(request.Perfis);
        if (perfis.IsFailure)
        {
            logger.LogInformation("Registro de usuário rejeitado pela validação de perfis: {ErrorCode}", perfis.Error.Code);
            return perfis.Error;
        }

        logger.LogInformation("Registro de usuário solicitado com {ProfileCount} perfil(is) válido(s)", perfis.Value.Count);

        // IgnoreQueryFilters: um usuário excluído logicamente ainda ocupa o e-mail (índice único), então também conflita.
        var emailNormalizado = userManager.NormalizeEmail(request.UsuarioEmail.Trim());
        var emailEmUso = await db.Usuarios
            .TagWith("Identidade.RegistrarUsuario.VerificarEmail")
            .IgnoreQueryFilters()
            .AnyAsync(u => u.NormalizedEmail == emailNormalizado || u.NormalizedUserName == emailNormalizado, cancellationToken);
        if (emailEmUso)
        {
            logger.LogInformation("Registro de usuário rejeitado porque o identificador de acesso já está em uso");
            return IdentidadeErros.EmailJaCadastrado;
        }

        logger.LogDebug("Chamando fábrica de domínio Usuario.Criar; o evento UsuarioRegistrado será anexado ao agregado");
        var usuario = Usuario.Criar(request.UsuarioNome, request.UsuarioEmail);

        return await db.ExecuteInTransactionAsync(async ct =>
        {
            logger.LogDebug("Chamando UserManager.CreateAsync para usuário {UsuarioId}", usuario.Id);
            var criacao = await userManager.CreateAsync(usuario, request.Senha);
            if (!criacao.Succeeded)
            {
                var erro = TraduzirFalha(criacao);
                logger.LogInformation("Identity rejeitou a criação do usuário pela regra {ErrorCode}; violações={IdentityErrorCount}", erro.Code, criacao.Errors.Count());
                return Result.Failure<RegistrarUsuarioResponse>(erro);
            }

            logger.LogDebug("Usuário {UsuarioId} criado; vinculando {ProfileCount} perfil(is)", usuario.Id, perfis.Value.Count);
            var vinculo = await userManager.AddToRolesAsync(usuario, perfis.Value);
            if (!vinculo.Succeeded)
            {
                // Perfis já foram validados contra PerfisPadrao e são criados pelo seed; falhar aqui é excepcional e desfaz a transação.
                logger.LogError("Falha inesperada ao vincular {ProfileCount} perfil(is) ao usuário {UsuarioId}; violações={IdentityErrorCount}", perfis.Value.Count, usuario.Id, vinculo.Errors.Count());
                throw new InvalidOperationException($"Falha ao vincular perfis ao usuário {usuario.Id}: {string.Join("; ", vinculo.Errors.Select(e => e.Description))}");
            }

            logger.LogInformation("Usuário {UsuarioId} registrado com {ProfileCount} perfil(is)", usuario.Id, perfis.Value.Count);
            return Result.Success(new RegistrarUsuarioResponse(usuario.Id, usuario.UsuarioNome, usuario.Email ?? string.Empty, perfis.Value));
        }, cancellationToken);
    }

    private Error TraduzirFalha(IdentityResult resultado)
    {
        var erros = resultado.Errors.ToList();
        if (erros.Any(e => e.Code is nameof(IdentityErrorDescriber.DuplicateEmail) or nameof(IdentityErrorDescriber.DuplicateUserName)))
        {
            logger.LogDebug("Falha do Identity traduzida como identificador de acesso duplicado");
            return IdentidadeErros.EmailJaCadastrado;
        }

        var senha = erros.Where(e => e.Code.StartsWith("Password", StringComparison.Ordinal)).Select(e => e.Description).ToList();
        if (senha.Count > 0)
        {
            logger.LogDebug("Falha do Identity traduzida como senha fora da política; violações={PasswordViolationCount}", senha.Count);
            return IdentidadeErros.SenhaFraca(senha);
        }

        logger.LogDebug("Falha do Identity traduzida como falha genérica de registro; violações={IdentityErrorCount}", erros.Count);
        return IdentidadeErros.FalhaAoRegistrar(erros.Select(e => e.Description));
    }
}
