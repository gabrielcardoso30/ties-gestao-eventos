using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Module.Palestras.Domain;
using Module.Palestras.Shared;
using Shared.Data.Extensions;
using Shared.Http.Endpoints;
using Shared.Http.Results;

namespace Module.Palestras.UseCases.ExcluirPalestra;

/// <summary>Exclusão lógica da palestra, de seus palestrantes e conteúdos (interceptor converte Remove em soft delete). Presenças e certificados permanecem.</summary>
internal sealed class ExcluirPalestraUseCase(PalestrasDbContext db, ILogger<ExcluirPalestraUseCase> logger) : IUseCase<ExcluirPalestraRequest, ExcluirPalestraResponse>
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
            logger.LogInformation("Palestra {TalkId} não encontrada para exclusão", request.PalestraId);
            return PalestrasErros.PalestraNaoEncontrada;
        }

        logger.LogDebug("Preparando exclusão lógica da palestra {TalkId}: palestrantes={SpeakerCount}, conteúdos={ContentCount}; presenças e certificados serão preservados", palestra.Id, palestra.Palestrantes.Count, palestra.Conteudos.Count);
        return await db.ExecuteInTransactionAsync(async ct =>
        {
            db.PalestraPalestrantes.RemoveRange(palestra.Palestrantes);
            db.PalestraConteudos.RemoveRange(palestra.Conteudos);
            db.Palestras.Remove(palestra);
            await db.SaveChangesAsync(ct);
            logger.LogInformation("Palestra {TalkId}, {SpeakerCount} palestrante(s) e {ContentCount} conteúdo(s) excluídos logicamente", palestra.Id, palestra.Palestrantes.Count, palestra.Conteudos.Count);
            return Result.Success(new ExcluirPalestraResponse(palestra.Id));
        }, cancellationToken);
    }
}
