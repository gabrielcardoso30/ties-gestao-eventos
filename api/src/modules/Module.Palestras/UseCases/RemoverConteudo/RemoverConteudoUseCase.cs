using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Module.Palestras.Domain;
using Module.Palestras.Shared;
using Shared.Data.Extensions;
using Shared.Http.Endpoints;
using Shared.Http.Results;

namespace Module.Palestras.UseCases.RemoverConteudo;

internal sealed class RemoverConteudoUseCase(PalestrasDbContext db, ILogger<RemoverConteudoUseCase> logger) : IUseCase<RemoverConteudoRequest, RemoverConteudoResponse>
{
    public async Task<Result<RemoverConteudoResponse>> HandleAsync(RemoverConteudoRequest request, CancellationToken cancellationToken)
    {
        var palestra = await db.Palestras
            .TagWith("Palestras.RemoverConteudo.Carregar")
            .Include(p => p.Conteudos)
            .FirstOrDefaultAsync(p => p.Id == request.PalestraId, cancellationToken);
        if (palestra is null)
        {
            logger.LogInformation("Remoção de conteúdo rejeitada: palestra {TalkId} não encontrada", request.PalestraId);
            return PalestrasErros.PalestraNaoEncontrada;
        }

        logger.LogDebug("Chamando agregado Palestra {TalkId} para remover conteúdo {ContentId}", palestra.Id, request.ConteudoId);
        var resultado = palestra.RemoverConteudo(request.ConteudoId);
        if (resultado.IsFailure)
        {
            logger.LogInformation("Agregado Palestra {TalkId} rejeitou remoção do conteúdo {ContentId} pela regra {ErrorCode}", palestra.Id, request.ConteudoId, resultado.Error.Code);
            return resultado.Error;
        }

        return await db.ExecuteInTransactionAsync(async ct =>
        {
            db.PalestraConteudos.Remove(resultado.Value);
            await db.SaveChangesAsync(ct);
            logger.LogInformation("Conteúdo {ContentId} removido da palestra {TalkId}; conteúdos restantes={ContentCount}", resultado.Value.Id, palestra.Id, palestra.Conteudos.Count);
            return Result.Success(new RemoverConteudoResponse(resultado.Value.Id));
        }, cancellationToken);
    }
}
