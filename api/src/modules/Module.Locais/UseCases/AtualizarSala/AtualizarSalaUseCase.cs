using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Module.Locais.Domain;
using Module.Locais.Shared;
using Shared.Data.Extensions;
using Shared.Http.Endpoints;
using Shared.Http.Results;

namespace Module.Locais.UseCases.AtualizarSala;

internal sealed class AtualizarSalaUseCase(LocaisDbContext db, ILogger<AtualizarSalaUseCase> logger) : IUseCase<AtualizarSalaRequest, AtualizarSalaResponse>
{
    public async Task<Result<AtualizarSalaResponse>> HandleAsync(AtualizarSalaRequest request, CancellationToken cancellationToken)
    {
        var local = await db.Locais
            .TagWith("Locais.AtualizarSala.CarregarLocal")
            .Include(l => l.Salas)
            .FirstOrDefaultAsync(l => l.Id == request.LocalId, cancellationToken);
        if (local is null)
        {
            logger.LogInformation("Atualização de sala rejeitada: local {LocalId} não encontrado", request.LocalId);
            return LocaisErros.LocalNaoEncontrado;
        }

        logger.LogDebug("Chamando agregado Local {LocalId} para atualizar sala {RoomId}", local.Id, request.SalaId);
        var resultado = local.AtualizarSala(request.SalaId, request.SalaNome, request.SalaCapacidade, request.SalaTipo, request.SalaRecursos);
        if (resultado.IsFailure)
        {
            logger.LogInformation("Agregado Local {LocalId} rejeitou atualização da sala {RoomId} pela regra {ErrorCode}", local.Id, request.SalaId, resultado.Error.Code);
            return resultado.Error;
        }

        var sala = local.Salas.First(s => s.Id == request.SalaId);
        sala.EstaAtivo = request.EstaAtivo;
        logger.LogDebug("Estado ativo da sala {RoomId} definido como {RoomActive}", sala.Id, sala.EstaAtivo);

        return await db.ExecuteInTransactionAsync(async ct =>
        {
            await db.SaveChangesAsync(ct);
            logger.LogInformation("Sala {RoomId} do local {LocalId} atualizada", sala.Id, sala.LocalId);
            return Result.Success(new AtualizarSalaResponse(sala.Id, sala.LocalId, sala.SalaNome, sala.SalaCapacidade, sala.SalaTipo, sala.EstaAtivo));
        }, cancellationToken);
    }
}
