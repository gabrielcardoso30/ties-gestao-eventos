using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Module.Identidade.Domain;
using Module.Identidade.Shared;
using Shared.Http.Endpoints;
using Shared.Http.Results;

namespace Module.Identidade.UseCases.ObterUsuarioAtual;

internal sealed class ObterUsuarioAtualUseCase(IdentidadeDbContext db, ILogger<ObterUsuarioAtualUseCase> logger) : IUseCase<ObterUsuarioAtualRequest, ObterUsuarioAtualResponse>
{
    public async Task<Result<ObterUsuarioAtualResponse>> HandleAsync(ObterUsuarioAtualRequest request, CancellationToken cancellationToken)
    {
        var usuario = await db.Usuarios
            .TagWith("Identidade.ObterUsuarioAtual")
            .AsNoTracking()
            .Where(u => u.Id == request.UsuarioId)
            .Select(u => new ObterUsuarioAtualResponse(
                u.Id,
                u.UsuarioNome,
                u.Email ?? string.Empty,
                db.UsuarioPerfis.Where(up => up.UserId == u.Id).Join(db.Perfis, up => up.RoleId, p => p.Id, (up, p) => p.Name!).OrderBy(nome => nome).ToList(),
                u.UltimoAcessoEm))
            .FirstOrDefaultAsync(cancellationToken);

        if (usuario is null)
        {
            logger.LogInformation("Usuário atual {UsuarioId} não encontrado", request.UsuarioId);
            return IdentidadeErros.UsuarioNaoEncontrado;
        }

        logger.LogInformation("Usuário atual {UsuarioId} carregado com {ProfileCount} perfil(is)", usuario.Id, usuario.Perfis.Count);
        return usuario;
    }
}
