using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Module.Locais.Domain;
using Module.Locais.Shared;
using Shared.Data.Extensions;
using Shared.Http.Endpoints;
using Shared.Http.Results;

namespace Module.Locais.UseCases.AdicionarSala;

internal sealed class AdicionarSalaUseCase(LocaisDbContext db, ILogger<AdicionarSalaUseCase> logger) : IUseCase<AdicionarSalaRequest, AdicionarSalaResponse>
{
    public async Task<Result<AdicionarSalaResponse>> HandleAsync(AdicionarSalaRequest request, CancellationToken cancellationToken)
    {
        var local = await db.Locais
            .TagWith("Locais.AdicionarSala.CarregarLocal")
            .Include(l => l.Salas)
            .FirstOrDefaultAsync(l => l.Id == request.LocalId, cancellationToken);
        if (local is null)
        {
            logger.LogInformation("Adição de sala rejeitada: local {LocalId} não encontrado", request.LocalId);
            return LocaisErros.LocalNaoEncontrado;
        }

        logger.LogDebug("Chamando agregado Local {LocalId} para adicionar sala; salas atuais={RoomCount}", local.Id, local.Salas.Count);
        var resultado = local.AdicionarSala(request.SalaNome, request.SalaCapacidade, request.SalaTipo, request.SalaRecursos);
        if (resultado.IsFailure)
        {
            logger.LogInformation("Agregado Local {LocalId} rejeitou nova sala pela regra {ErrorCode}", local.Id, resultado.Error.Code);
            return resultado.Error;
        }

        var sala = resultado.Value;
        return await db.ExecuteInTransactionAsync(async ct =>
        {
            await db.SaveChangesAsync(ct);
            logger.LogInformation("Sala {RoomId} adicionada ao local {LocalId}; total de salas={RoomCount}", sala.Id, sala.LocalId, local.Salas.Count);
            return Result.Success(new AdicionarSalaResponse(sala.Id, sala.LocalId, sala.SalaNome, sala.SalaCapacidade, sala.SalaTipo));
        }, cancellationToken);
    }
}
