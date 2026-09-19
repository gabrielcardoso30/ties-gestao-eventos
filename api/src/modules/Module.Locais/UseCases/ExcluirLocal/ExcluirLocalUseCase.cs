using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Module.Locais.Domain;
using Module.Locais.Shared;
using Shared.Data.Extensions;
using Shared.Http.Endpoints;
using Shared.Http.Results;

namespace Module.Locais.UseCases.ExcluirLocal;

/// <summary>Exclusão lógica do local e de suas salas (interceptor converte Remove em soft delete).</summary>
internal sealed class ExcluirLocalUseCase(LocaisDbContext db, ILogger<ExcluirLocalUseCase> logger) : IUseCase<ExcluirLocalRequest, ExcluirLocalResponse>
{
    public async Task<Result<ExcluirLocalResponse>> HandleAsync(ExcluirLocalRequest request, CancellationToken cancellationToken)
    {
        var local = await db.Locais
            .TagWith("Locais.ExcluirLocal.Carregar")
            .Include(l => l.Salas)
            .FirstOrDefaultAsync(l => l.Id == request.LocalId, cancellationToken);
        if (local is null)
        {
            logger.LogInformation("Local {LocalId} não encontrado para exclusão", request.LocalId);
            return LocaisErros.LocalNaoEncontrado;
        }

        logger.LogDebug("Chamando agregado Local {LocalId} para marcar exclusão lógica; salas vinculadas={RoomCount}", local.Id, local.Salas.Count);
        local.MarcarExcluido();
        return await db.ExecuteInTransactionAsync(async ct =>
        {
            db.Salas.RemoveRange(local.Salas);
            db.Locais.Remove(local);
            await db.SaveChangesAsync(ct);
            logger.LogInformation("Local {LocalId} e {RoomCount} sala(s) excluídos logicamente", local.Id, local.Salas.Count);
            return Result.Success(new ExcluirLocalResponse(local.Id));
        }, cancellationToken);
    }
}
