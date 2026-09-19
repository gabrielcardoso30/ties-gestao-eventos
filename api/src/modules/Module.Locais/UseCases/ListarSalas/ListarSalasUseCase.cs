using Microsoft.EntityFrameworkCore;
using Module.Locais.Domain;
using Module.Locais.Shared;
using Shared.Http.Endpoints;
using Shared.Http.Results;

namespace Module.Locais.UseCases.ListarSalas;

internal sealed class ListarSalasUseCase(LocaisDbContext db) : IUseCase<ListarSalasRequest, IReadOnlyList<ListarSalasItemResponse>>
{
    public async Task<Result<IReadOnlyList<ListarSalasItemResponse>>> HandleAsync(ListarSalasRequest request, CancellationToken cancellationToken)
    {
        var localExiste = await db.Locais.TagWith("Locais.ListarSalas.VerificarLocal").AnyAsync(l => l.Id == request.LocalId, cancellationToken);
        if (!localExiste)
        {
            return LocaisErros.LocalNaoEncontrado;
        }

        var query = db.Salas.TagWith("Locais.ListarSalas").AsNoTracking().Where(s => s.LocalId == request.LocalId);
        if (request.EstaAtivo.HasValue)
        {
            query = query.Where(s => s.EstaAtivo == request.EstaAtivo.Value);
        }

        var salas = await query
            .OrderBy(s => s.SalaNome)
            .Select(s => new ListarSalasItemResponse(s.Id, s.SalaNome, s.SalaCapacidade, s.SalaTipo, s.SalaRecursos, s.EstaAtivo))
            .ToListAsync(cancellationToken);

        return salas;
    }
}
