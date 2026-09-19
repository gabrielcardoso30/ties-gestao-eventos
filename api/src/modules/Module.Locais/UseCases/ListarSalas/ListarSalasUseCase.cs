using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Module.Locais.Domain;
using Module.Locais.Shared;
using Shared.Http.Endpoints;
using Shared.Http.Results;

namespace Module.Locais.UseCases.ListarSalas;

internal sealed class ListarSalasUseCase(LocaisDbContext db, ILogger<ListarSalasUseCase> logger) : IUseCase<ListarSalasRequest, IReadOnlyList<ListarSalasItemResponse>>
{
    public async Task<Result<IReadOnlyList<ListarSalasItemResponse>>> HandleAsync(ListarSalasRequest request, CancellationToken cancellationToken)
    {
        var localExiste = await db.Locais.TagWith("Locais.ListarSalas.VerificarLocal").AnyAsync(l => l.Id == request.LocalId, cancellationToken);
        if (!localExiste)
        {
            logger.LogInformation("Listagem de salas rejeitada: local {LocalId} não encontrado", request.LocalId);
            return LocaisErros.LocalNaoEncontrado;
        }

        var query = db.Salas.TagWith("Locais.ListarSalas").AsNoTracking().Where(s => s.LocalId == request.LocalId);
        if (request.EstaAtivo.HasValue)
        {
            query = query.Where(s => s.EstaAtivo == request.EstaAtivo.Value);
            logger.LogDebug("Filtro de ativo={Active} aplicado às salas do local {LocalId}", request.EstaAtivo, request.LocalId);
        }
        else
        {
            logger.LogDebug("Listagem de salas do local {LocalId} inclui ativas e inativas", request.LocalId);
        }

        var salas = await query
            .OrderBy(s => s.SalaNome)
            .Select(s => new ListarSalasItemResponse(s.Id, s.SalaNome, s.SalaCapacidade, s.SalaTipo, s.SalaRecursos, s.EstaAtivo))
            .ToListAsync(cancellationToken);

        logger.LogInformation("Local {LocalId} possui {RoomCount} sala(s) no filtro solicitado", request.LocalId, salas.Count);

        return salas;
    }
}
