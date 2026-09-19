using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Module.Locais.Domain;
using Module.Locais.Shared;
using Shared.Data.Extensions;
using Shared.Http.Endpoints;
using Shared.Http.Results;

namespace Module.Locais.UseCases.ExcluirSala;

internal sealed class ExcluirSalaUseCase(LocaisDbContext db, ILogger<ExcluirSalaUseCase> logger) : IUseCase<ExcluirSalaRequest, ExcluirSalaResponse>
{
    public async Task<Result<ExcluirSalaResponse>> HandleAsync(ExcluirSalaRequest request, CancellationToken cancellationToken)
    {
        var local = await db.Locais
            .TagWith("Locais.ExcluirSala.CarregarLocal")
            .Include(l => l.Salas)
            .FirstOrDefaultAsync(l => l.Id == request.LocalId, cancellationToken);
        if (local is null)
        {
            logger.LogInformation("Exclusão de sala rejeitada: local {LocalId} não encontrado", request.LocalId);
            return LocaisErros.LocalNaoEncontrado;
        }

        logger.LogDebug("Chamando agregado Local {LocalId} para remover sala {RoomId}", local.Id, request.SalaId);
        var resultado = local.RemoverSala(request.SalaId);
        if (resultado.IsFailure)
        {
            logger.LogInformation("Agregado Local {LocalId} rejeitou remoção da sala {RoomId} pela regra {ErrorCode}", local.Id, request.SalaId, resultado.Error.Code);
            return resultado.Error;
        }

        return await db.ExecuteInTransactionAsync(async ct =>
        {
            db.Salas.Remove(resultado.Value);
            await db.SaveChangesAsync(ct);
            logger.LogInformation("Sala {RoomId} do local {LocalId} excluída logicamente", resultado.Value.Id, local.Id);
            return Result.Success(new ExcluirSalaResponse(resultado.Value.Id));
        }, cancellationToken);
    }
}
