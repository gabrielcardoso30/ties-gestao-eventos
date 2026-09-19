using Microsoft.EntityFrameworkCore;
using Module.Locais.Domain;
using Module.Locais.Shared;
using Shared.Data.Extensions;
using Shared.Http.Endpoints;
using Shared.Http.Results;

namespace Module.Locais.UseCases.ExcluirLocal;

/// <summary>Exclusão lógica do local e de suas salas (interceptor converte Remove em soft delete).</summary>
internal sealed class ExcluirLocalUseCase(LocaisDbContext db) : IUseCase<ExcluirLocalRequest, ExcluirLocalResponse>
{
    public async Task<Result<ExcluirLocalResponse>> HandleAsync(ExcluirLocalRequest request, CancellationToken cancellationToken)
    {
        var local = await db.Locais
            .TagWith("Locais.ExcluirLocal.Carregar")
            .Include(l => l.Salas)
            .FirstOrDefaultAsync(l => l.Id == request.LocalId, cancellationToken);
        if (local is null)
        {
            return LocaisErros.LocalNaoEncontrado;
        }

        local.MarcarExcluido();
        return await db.ExecuteInTransactionAsync(async ct =>
        {
            db.Salas.RemoveRange(local.Salas);
            db.Locais.Remove(local);
            await db.SaveChangesAsync(ct);
            return Result.Success(new ExcluirLocalResponse(local.Id));
        }, cancellationToken);
    }
}
