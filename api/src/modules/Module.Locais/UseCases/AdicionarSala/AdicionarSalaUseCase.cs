using Microsoft.EntityFrameworkCore;
using Module.Locais.Domain;
using Module.Locais.Shared;
using Shared.Data.Extensions;
using Shared.Http.Endpoints;
using Shared.Http.Results;

namespace Module.Locais.UseCases.AdicionarSala;

internal sealed class AdicionarSalaUseCase(LocaisDbContext db) : IUseCase<AdicionarSalaRequest, AdicionarSalaResponse>
{
    public async Task<Result<AdicionarSalaResponse>> HandleAsync(AdicionarSalaRequest request, CancellationToken cancellationToken)
    {
        var local = await db.Locais
            .TagWith("Locais.AdicionarSala.CarregarLocal")
            .Include(l => l.Salas)
            .FirstOrDefaultAsync(l => l.Id == request.LocalId, cancellationToken);
        if (local is null)
        {
            return LocaisErros.LocalNaoEncontrado;
        }

        var resultado = local.AdicionarSala(request.SalaNome, request.SalaCapacidade, request.SalaTipo, request.SalaRecursos);
        if (resultado.IsFailure)
        {
            return resultado.Error;
        }

        var sala = resultado.Value;
        return await db.ExecuteInTransactionAsync(async ct =>
        {
            await db.SaveChangesAsync(ct);
            return Result.Success(new AdicionarSalaResponse(sala.Id, sala.LocalId, sala.SalaNome, sala.SalaCapacidade, sala.SalaTipo));
        }, cancellationToken);
    }
}
