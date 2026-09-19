using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Module.Palestras.Domain;
using Module.Palestras.Shared;
using Shared.Data.Extensions;
using Shared.Http.Endpoints;
using Shared.Http.Results;

namespace Module.Palestras.UseCases.AtualizarPalestra;

/// <summary>Atualiza dados e agenda da palestra, revalidando evento, período e sala (ignorando a própria palestra na sobreposição).</summary>
internal sealed class AtualizarPalestraUseCase(PalestrasDbContext db, AgendaPalestraVerificador agenda, ILogger<AtualizarPalestraUseCase> logger) : IUseCase<AtualizarPalestraRequest, AtualizarPalestraResponse>
{
    public async Task<Result<AtualizarPalestraResponse>> HandleAsync(AtualizarPalestraRequest request, CancellationToken cancellationToken)
    {
        var palestra = await db.Palestras
            .TagWith("Palestras.AtualizarPalestra.Carregar")
            .FirstOrDefaultAsync(p => p.Id == request.PalestraId, cancellationToken);
        if (palestra is null)
        {
            logger.LogInformation("Palestra {TalkId} não encontrada para atualização", request.PalestraId);
            return PalestrasErros.PalestraNaoEncontrada;
        }

        logger.LogDebug("Revalidando agenda da palestra {TalkId} no evento {EventoId}, trilha {TrackId} e sala {RoomId}", palestra.Id, palestra.EventoId, request.TrilhaId, request.SalaId);
        var verificacao = await agenda.VerificarAsync(palestra.EventoId, request.TrilhaId, request.SalaId, request.PalestraInicio, request.PalestraFim, cancellationToken);
        if (verificacao.IsFailure)
        {
            logger.LogInformation("Atualização da palestra {TalkId} rejeitada pela regra de agenda {ErrorCode}", palestra.Id, verificacao.Error.Code);
            return verificacao.Error;
        }

        if (request.SalaId.HasValue)
        {
            logger.LogDebug("Verificando sobreposição da sala {RoomId} para palestra {TalkId}", request.SalaId, palestra.Id);
            var salaOcupada = await db.Palestras
                .TagWith("Palestras.AtualizarPalestra.VerificarSala")
                .AnyAsync(PalestraAgenda.OcupaSala(request.SalaId.Value, request.PalestraInicio, request.PalestraFim, palestra.Id), cancellationToken);
            if (salaOcupada)
            {
                logger.LogInformation("Atualização da palestra {TalkId} rejeitada porque a sala {RoomId} está ocupada", palestra.Id, request.SalaId);
                return PalestrasErros.SalaOcupada;
            }
            logger.LogDebug("Sala {RoomId} disponível no período solicitado para palestra {TalkId}", request.SalaId, palestra.Id);
        }
        else
        {
            logger.LogDebug("Palestra {TalkId} será atualizada sem sala; verificação de sobreposição ignorada", palestra.Id);
        }

        logger.LogDebug("Chamando agregado Palestra {TalkId}.Atualizar", palestra.Id);
        palestra.Atualizar(request.TrilhaId, request.SalaId, request.PalestraTitulo, request.PalestraDescricao, request.PalestraInicio, request.PalestraFim);

        return await db.ExecuteInTransactionAsync(async ct =>
        {
            await db.SaveChangesAsync(ct);
            logger.LogInformation("Palestra {TalkId} atualizada na trilha {TrackId} e sala {RoomId}", palestra.Id, palestra.TrilhaId, palestra.SalaId);
            return Result.Success(new AtualizarPalestraResponse(palestra.Id, palestra.PalestraTitulo, palestra.AlteradoEm));
        }, cancellationToken);
    }
}
