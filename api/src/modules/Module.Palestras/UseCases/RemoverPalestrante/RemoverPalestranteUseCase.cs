using Microsoft.EntityFrameworkCore;
using Module.Palestras.Domain;
using Module.Palestras.Shared;
using Shared.Data.Extensions;
using Shared.Http.Endpoints;
using Shared.Http.Results;

namespace Module.Palestras.UseCases.RemoverPalestrante;

internal sealed class RemoverPalestranteUseCase(PalestrasDbContext db) : IUseCase<RemoverPalestranteRequest, RemoverPalestranteResponse>
{
    public async Task<Result<RemoverPalestranteResponse>> HandleAsync(RemoverPalestranteRequest request, CancellationToken cancellationToken)
    {
        var palestra = await db.Palestras
            .TagWith("Palestras.RemoverPalestrante.Carregar")
            .Include(p => p.Palestrantes)
            .FirstOrDefaultAsync(p => p.Id == request.PalestraId, cancellationToken);
        if (palestra is null)
        {
            return PalestrasErros.PalestraNaoEncontrada;
        }

        var resultado = palestra.RemoverPalestrante(request.PessoaId);
        if (resultado.IsFailure)
        {
            return resultado.Error;
        }

        return await db.ExecuteInTransactionAsync(async ct =>
        {
            db.PalestraPalestrantes.Remove(resultado.Value);
            await db.SaveChangesAsync(ct);
            return Result.Success(new RemoverPalestranteResponse(palestra.Id, request.PessoaId));
        }, cancellationToken);
    }
}
