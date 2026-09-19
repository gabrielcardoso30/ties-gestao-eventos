using Microsoft.EntityFrameworkCore;
using Module.Locais.Domain;
using Module.Locais.Shared;
using Shared.Data.Extensions;
using Shared.Http.Endpoints;
using Shared.Http.Results;

namespace Module.Locais.UseCases.AtualizarSala;

internal sealed class AtualizarSalaUseCase(LocaisDbContext db) : IUseCase<AtualizarSalaRequest, AtualizarSalaResponse>
{
    public async Task<Result<AtualizarSalaResponse>> HandleAsync(AtualizarSalaRequest request, CancellationToken cancellationToken)
    {
        var local = await db.Locais
            .TagWith("Locais.AtualizarSala.CarregarLocal")
            .Include(l => l.Salas)
            .FirstOrDefaultAsync(l => l.Id == request.LocalId, cancellationToken);
        if (local is null)
        {
            return LocaisErros.LocalNaoEncontrado;
        }

        var resultado = local.AtualizarSala(request.SalaId, request.SalaNome, request.SalaCapacidade, request.SalaTipo, request.SalaRecursos);
        if (resultado.IsFailure)
        {
            return resultado.Error;
        }

        var sala = local.Salas.First(s => s.Id == request.SalaId);
        sala.EstaAtivo = request.EstaAtivo;

        return await db.ExecuteInTransactionAsync(async ct =>
        {
            await db.SaveChangesAsync(ct);
            return Result.Success(new AtualizarSalaResponse(sala.Id, sala.LocalId, sala.SalaNome, sala.SalaCapacidade, sala.SalaTipo, sala.EstaAtivo));
        }, cancellationToken);
    }
}
