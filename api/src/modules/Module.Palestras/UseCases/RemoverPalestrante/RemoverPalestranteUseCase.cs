using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Module.Palestras.Domain;
using Module.Palestras.Shared;
using Shared.Data.Extensions;
using Shared.Http.Endpoints;
using Shared.Http.Results;

namespace Module.Palestras.UseCases.RemoverPalestrante;

internal sealed class RemoverPalestranteUseCase(PalestrasDbContext db, ILogger<RemoverPalestranteUseCase> logger) : IUseCase<RemoverPalestranteRequest, RemoverPalestranteResponse>
{
    public async Task<Result<RemoverPalestranteResponse>> HandleAsync(RemoverPalestranteRequest request, CancellationToken cancellationToken)
    {
        var palestra = await db.Palestras
            .TagWith("Palestras.RemoverPalestrante.Carregar")
            .Include(p => p.Palestrantes)
            .FirstOrDefaultAsync(p => p.Id == request.PalestraId, cancellationToken);
        if (palestra is null)
        {
            logger.LogInformation("Remoção de palestrante rejeitada: palestra {TalkId} não encontrada", request.PalestraId);
            return PalestrasErros.PalestraNaoEncontrada;
        }

        logger.LogDebug("Chamando agregado Palestra {TalkId} para remover pessoa {PessoaId}; palestrantes atuais={SpeakerCount}", palestra.Id, request.PessoaId, palestra.Palestrantes.Count);
        var resultado = palestra.RemoverPalestrante(request.PessoaId);
        if (resultado.IsFailure)
        {
            logger.LogInformation("Agregado Palestra {TalkId} rejeitou remoção da pessoa {PessoaId} pela regra {ErrorCode}", palestra.Id, request.PessoaId, resultado.Error.Code);
            return resultado.Error;
        }

        return await db.ExecuteInTransactionAsync(async ct =>
        {
            db.PalestraPalestrantes.Remove(resultado.Value);
            await db.SaveChangesAsync(ct);
            logger.LogInformation("Pessoa {PessoaId} removida da palestra {TalkId}; palestrantes restantes={SpeakerCount}", request.PessoaId, palestra.Id, palestra.Palestrantes.Count);
            return Result.Success(new RemoverPalestranteResponse(palestra.Id, request.PessoaId));
        }, cancellationToken);
    }
}
