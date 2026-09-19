using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Module.Identidade.Domain;
using Module.Identidade.Shared;
using Shared.Data.Extensions;
using Shared.Http.Endpoints;
using Shared.Http.Results;

namespace Module.Identidade.UseCases.AtualizarPerfisUsuario;

/// <summary>Substitui o conjunto de perfis do usuário (diff em <c>UsuarioPerfis</c>: remove os ausentes, adiciona os novos). Lista vazia remove todos.</summary>
internal sealed class AtualizarPerfisUsuarioUseCase(IdentidadeDbContext db, ILogger<AtualizarPerfisUsuarioUseCase> logger) : IUseCase<AtualizarPerfisUsuarioRequest, AtualizarPerfisUsuarioResponse>
{
    public async Task<Result<AtualizarPerfisUsuarioResponse>> HandleAsync(AtualizarPerfisUsuarioRequest request, CancellationToken cancellationToken)
    {
        logger.LogDebug("Chamando regra de domínio PerfisValidos.Normalizar para {ProfileCount} perfil(is) solicitado(s)", request.Perfis.Count);
        var perfis = PerfisValidos.Normalizar(request.Perfis);
        if (perfis.IsFailure)
        {
            logger.LogInformation("Atualização de perfis do usuário {UsuarioId} rejeitada pela regra {ErrorCode}", request.UsuarioId, perfis.Error.Code);
            return perfis.Error;
        }

        var usuario = await db.Usuarios
            .TagWith("Identidade.AtualizarPerfisUsuario.CarregarUsuario")
            .FirstOrDefaultAsync(u => u.Id == request.UsuarioId, cancellationToken);
        if (usuario is null)
        {
            logger.LogInformation("Usuário {UsuarioId} não encontrado para atualização de perfis", request.UsuarioId);
            return IdentidadeErros.UsuarioNaoEncontrado;
        }

        var desejados = perfis.Value;
        logger.LogDebug("Carregando {DesiredProfileCount} perfil(is) normalizado(s) para validar existência", desejados.Count);
        var perfisExistentes = await db.Perfis
            .TagWith("Identidade.AtualizarPerfisUsuario.CarregarPerfis")
            .AsNoTracking()
            .Where(p => desejados.Contains(p.Name!))
            .Select(p => new { p.Id, p.Name })
            .ToListAsync(cancellationToken);
        var naoEncontrado = desejados.FirstOrDefault(d => perfisExistentes.All(p => p.Name != d));
        if (naoEncontrado is not null)
        {
            logger.LogInformation("Atualização de perfis do usuário {UsuarioId} rejeitada porque um dos {DesiredProfileCount} perfis não existe", usuario.Id, desejados.Count);
            return IdentidadeErros.PerfilInvalido(naoEncontrado);
        }
        logger.LogDebug("Todos os {DesiredProfileCount} perfil(is) desejados existem", desejados.Count);

        var vinculosAtuais = await db.UsuarioPerfis
            .TagWith("Identidade.AtualizarPerfisUsuario.CarregarVinculos")
            .Where(up => up.UserId == usuario.Id)
            .ToListAsync(cancellationToken);

        var idsDesejados = perfisExistentes.Select(p => p.Id).ToHashSet();
        var remover = vinculosAtuais.Where(v => !idsDesejados.Contains(v.RoleId)).ToList();
        var adicionar = idsDesejados.Where(id => vinculosAtuais.All(v => v.RoleId != id))
            .Select(id => new UsuarioPerfil { UserId = usuario.Id, RoleId = id })
            .ToList();

        logger.LogInformation("Diferença de perfis do usuário {UsuarioId}: atuais={CurrentProfileCount}, desejados={DesiredProfileCount}, adicionar={ProfilesToAdd}, remover={ProfilesToRemove}",
            usuario.Id, vinculosAtuais.Count, desejados.Count, adicionar.Count, remover.Count);

        // Perfis mudaram: renova o ConcurrencyStamp para que o usuário apareça na trilha de auditoria (EntidadeAlterada).
        if (remover.Count > 0 || adicionar.Count > 0)
        {
            logger.LogDebug("Renovando selo de concorrência do usuário {UsuarioId} para registrar alteração de perfis na auditoria", usuario.Id);
            usuario.ConcurrencyStamp = Guid.NewGuid().ToString();
        }
        else
        {
            logger.LogDebug("Perfis do usuário {UsuarioId} já correspondem ao estado desejado; nenhuma alteração de vínculo necessária", usuario.Id);
        }

        return await db.ExecuteInTransactionAsync(async ct =>
        {
            db.UsuarioPerfis.RemoveRange(remover);
            db.UsuarioPerfis.AddRange(adicionar);
            await db.SaveChangesAsync(ct);
            logger.LogInformation("Perfis do usuário {UsuarioId} persistidos; total desejado={DesiredProfileCount}", usuario.Id, desejados.Count);
            return Result.Success(new AtualizarPerfisUsuarioResponse(usuario.Id, desejados));
        }, cancellationToken);
    }
}
