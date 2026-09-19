using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Module.Palestras.Domain;
using Module.Palestras.Shared;
using Shared.Contracts.Pessoas;
using Shared.Data.Extensions;
using Shared.Http.Endpoints;
using Shared.Http.Results;

namespace Module.Palestras.UseCases.AdicionarPalestrante;

internal sealed class AdicionarPalestranteUseCase(PalestrasDbContext db, IPessoasModuleApi pessoasApi, ILogger<AdicionarPalestranteUseCase> logger) : IUseCase<AdicionarPalestranteRequest, AdicionarPalestranteResponse>
{
    public async Task<Result<AdicionarPalestranteResponse>> HandleAsync(AdicionarPalestranteRequest request, CancellationToken cancellationToken)
    {
        var palestra = await db.Palestras
            .TagWith("Palestras.AdicionarPalestrante.Carregar")
            .Include(p => p.Palestrantes)
            .FirstOrDefaultAsync(p => p.Id == request.PalestraId, cancellationToken);
        if (palestra is null)
        {
            logger.LogInformation("Adição de palestrante rejeitada: palestra {TalkId} não encontrada", request.PalestraId);
            return PalestrasErros.PalestraNaoEncontrada;
        }

        logger.LogDebug("Consultando módulo Pessoas para validar palestrante {PessoaId}", request.PessoaId);
        var pessoa = await pessoasApi.ObterPessoaResumoAsync(request.PessoaId, cancellationToken);
        if (pessoa is null)
        {
            logger.LogInformation("Adição de palestrante à palestra {TalkId} rejeitada: pessoa {PessoaId} não encontrada", palestra.Id, request.PessoaId);
            return PalestrasErros.PessoaNaoEncontrada;
        }

        logger.LogDebug("Chamando agregado Palestra {TalkId} para adicionar pessoa {PessoaId} no papel {SpeakerRole}", palestra.Id, request.PessoaId, request.PalestrantePapel);
        var resultado = palestra.AdicionarPalestrante(request.PessoaId, request.PalestrantePapel);
        if (resultado.IsFailure)
        {
            logger.LogInformation("Agregado Palestra {TalkId} rejeitou palestrante {PessoaId} pela regra {ErrorCode}", palestra.Id, request.PessoaId, resultado.Error.Code);
            return resultado.Error;
        }

        var palestrante = resultado.Value;
        return await db.ExecuteInTransactionAsync(async ct =>
        {
            await db.SaveChangesAsync(ct);
            logger.LogInformation("Pessoa {PessoaId} adicionada à palestra {TalkId}; total de palestrantes={SpeakerCount}", palestrante.PessoaId, palestra.Id, palestra.Palestrantes.Count);
            return Result.Success(new AdicionarPalestranteResponse(palestrante.PalestraId, palestrante.PessoaId, palestrante.PalestrantePapel));
        }, cancellationToken);
    }
}
