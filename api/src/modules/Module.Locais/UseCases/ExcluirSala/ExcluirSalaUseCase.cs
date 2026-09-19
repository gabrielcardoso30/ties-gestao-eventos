using Microsoft.EntityFrameworkCore;
using Module.Locais.Domain;
using Module.Locais.Shared;
using Shared.Data.Extensions;
using Shared.Http.Endpoints;
using Shared.Http.Results;

namespace Module.Locais.UseCases.ExcluirSala;

internal sealed class ExcluirSalaUseCase(LocaisDbContext db) : IUseCase<ExcluirSalaRequest, ExcluirSalaResponse>
{
    public async Task<Result<ExcluirSalaResponse>> HandleAsync(ExcluirSalaRequest request, CancellationToken cancellationToken)
    {
        var local = await db.Locais
            .TagWith("Locais.ExcluirSala.CarregarLocal")
            .Include(l => l.Salas)
            .FirstOrDefaultAsync(l => l.Id == request.LocalId, cancellationToken);
        if (local is null)
        {
            return LocaisErros.LocalNaoEncontrado;
        }

        var resultado = local.RemoverSala(request.SalaId);
        if (resultado.IsFailure)
        {
            return resultado.Error;
        }

        return await db.ExecuteInTransactionAsync(async ct =>
        {
            db.Salas.Remove(resultado.Value);
            await db.SaveChangesAsync(ct);
            return Result.Success(new ExcluirSalaResponse(resultado.Value.Id));
        }, cancellationToken);
    }
}
