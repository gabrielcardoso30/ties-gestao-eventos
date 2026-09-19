using Microsoft.EntityFrameworkCore;
using Module.Palestras.Domain;
using Module.Palestras.Shared;
using Shared.Data.Extensions;
using Shared.Http.Endpoints;
using Shared.Http.Results;

namespace Module.Palestras.UseCases.ExcluirPalestra;

/// <summary>Exclusão lógica da palestra, de seus palestrantes e conteúdos (interceptor converte Remove em soft delete). Presenças e certificados permanecem.</summary>
internal sealed class ExcluirPalestraUseCase(PalestrasDbContext db) : IUseCase<ExcluirPalestraRequest, ExcluirPalestraResponse>
{
    public async Task<Result<ExcluirPalestraResponse>> HandleAsync(ExcluirPalestraRequest request, CancellationToken cancellationToken)
    {
        var palestra = await db.Palestras
            .TagWith("Palestras.ExcluirPalestra.Carregar")
            .Include(p => p.Palestrantes)
            .Include(p => p.Conteudos)
            .FirstOrDefaultAsync(p => p.Id == request.PalestraId, cancellationToken);
        if (palestra is null)
        {
            return PalestrasErros.PalestraNaoEncontrada;
        }

        return await db.ExecuteInTransactionAsync(async ct =>
        {
            db.PalestraPalestrantes.RemoveRange(palestra.Palestrantes);
            db.PalestraConteudos.RemoveRange(palestra.Conteudos);
            db.Palestras.Remove(palestra);
            await db.SaveChangesAsync(ct);
            return Result.Success(new ExcluirPalestraResponse(palestra.Id));
        }, cancellationToken);
    }
}
