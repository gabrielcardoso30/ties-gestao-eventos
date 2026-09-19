using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
internal sealed class RegistrarUsuarioUseCase(IdentidadeDbContext db, UserManager<Usuario> userManager) : IUseCase<RegistrarUsuarioRequest, RegistrarUsuarioResponse>
{
    public async Task<Result<RegistrarUsuarioResponse>> HandleAsync(RegistrarUsuarioRequest request, CancellationToken cancellationToken)
    {
        var perfis = PerfisValidos.Normalizar(request.Perfis);
        if (perfis.IsFailure)
        {
            return perfis.Error;
        }

        // IgnoreQueryFilters: um usuário excluído logicamente ainda ocupa o e-mail (índice único), então também conflita.
        var emailNormalizado = userManager.NormalizeEmail(request.UsuarioEmail.Trim());
        var emailEmUso = await db.Usuarios
            .TagWith("Identidade.RegistrarUsuario.VerificarEmail")
            .IgnoreQueryFilters()
            .AnyAsync(u => u.NormalizedEmail == emailNormalizado || u.NormalizedUserName == emailNormalizado, cancellationToken);
        if (emailEmUso)
        {
            return IdentidadeErros.EmailJaCadastrado;
        }

        var usuario = Usuario.Criar(request.UsuarioNome, request.UsuarioEmail);

        return await db.ExecuteInTransactionAsync(async ct =>
        {
            var criacao = await userManager.CreateAsync(usuario, request.Senha);
            if (!criacao.Succeeded)
            {
                return Result.Failure<RegistrarUsuarioResponse>(TraduzirFalha(criacao));
            }

            var vinculo = await userManager.AddToRolesAsync(usuario, perfis.Value);
            if (!vinculo.Succeeded)
            {
                // Perfis já foram validados contra PerfisPadrao e são criados pelo seed; falhar aqui é excepcional e desfaz a transação.
                throw new InvalidOperationException($"Falha ao vincular perfis ao usuário {usuario.Id}: {string.Join("; ", vinculo.Errors.Select(e => e.Description))}");
            }

            return Result.Success(new RegistrarUsuarioResponse(usuario.Id, usuario.UsuarioNome, usuario.Email ?? string.Empty, perfis.Value));
        }, cancellationToken);
    }

    private static Error TraduzirFalha(IdentityResult resultado)
    {
        var erros = resultado.Errors.ToList();
        if (erros.Any(e => e.Code is nameof(IdentityErrorDescriber.DuplicateEmail) or nameof(IdentityErrorDescriber.DuplicateUserName)))
        {
            return IdentidadeErros.EmailJaCadastrado;
        }

        var senha = erros.Where(e => e.Code.StartsWith("Password", StringComparison.Ordinal)).Select(e => e.Description).ToList();
        return senha.Count > 0
            ? IdentidadeErros.SenhaFraca(senha)
            : IdentidadeErros.FalhaAoRegistrar(erros.Select(e => e.Description));
    }
}
