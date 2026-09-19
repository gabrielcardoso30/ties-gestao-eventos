using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Module.Palestras.Domain;
using Module.Palestras.Shared;
using Shared.Data.Extensions;
using Shared.Http.Endpoints;
using Shared.Http.Results;

namespace Module.Palestras.UseCases.AdicionarConteudo;

internal sealed class AdicionarConteudoUseCase(PalestrasDbContext db, ILogger<AdicionarConteudoUseCase> logger) : IUseCase<AdicionarConteudoRequest, AdicionarConteudoResponse>
{
    public async Task<Result<AdicionarConteudoResponse>> HandleAsync(AdicionarConteudoRequest request, CancellationToken cancellationToken)
    {
        var palestra = await db.Palestras
            .TagWith("Palestras.AdicionarConteudo.Carregar")
            .Include(p => p.Conteudos)
            .FirstOrDefaultAsync(p => p.Id == request.PalestraId, cancellationToken);
        if (palestra is null)
        {
            logger.LogInformation("Adição de conteúdo rejeitada: palestra {TalkId} não encontrada", request.PalestraId);
            return PalestrasErros.PalestraNaoEncontrada;
        }

        logger.LogDebug("Chamando agregado Palestra {TalkId} para adicionar conteúdo do tipo {ContentType}; conteúdos atuais={ContentCount}", palestra.Id, request.ConteudoTipo, palestra.Conteudos.Count);
        var conteudo = palestra.AdicionarConteudo(request.ConteudoTitulo, request.ConteudoTipo, request.ConteudoUrl, request.ConteudoDescricao);

        return await db.ExecuteInTransactionAsync(async ct =>
        {
            await db.SaveChangesAsync(ct);
            logger.LogInformation("Conteúdo {ContentId} adicionado à palestra {TalkId}; total de conteúdos={ContentCount}", conteudo.Id, palestra.Id, palestra.Conteudos.Count);
            return Result.Success(new AdicionarConteudoResponse(conteudo.Id, conteudo.PalestraId, conteudo.ConteudoTitulo, conteudo.ConteudoTipo, conteudo.ConteudoUrl));
        }, cancellationToken);
    }
}
