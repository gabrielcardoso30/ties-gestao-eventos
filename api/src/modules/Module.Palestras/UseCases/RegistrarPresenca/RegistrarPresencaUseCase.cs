using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Module.Palestras.Domain;
using Module.Palestras.Shared;
using Shared.Contracts.Eventos;
using Shared.Contracts.Palestras;
using Shared.Data.Extensions;
using Shared.Http.Endpoints;
using Shared.Http.Results;

namespace Module.Palestras.UseCases.RegistrarPresenca;

/// <summary>Registra presença de um participante com inscrição confirmada no evento. Emite <see cref="PresencaRegistrada"/>.</summary>
internal sealed class RegistrarPresencaUseCase(PalestrasDbContext db, IEventosModuleApi eventosApi, TimeProvider timeProvider, ILogger<RegistrarPresencaUseCase> logger) : IUseCase<RegistrarPresencaRequest, RegistrarPresencaResponse>
{
    public async Task<Result<RegistrarPresencaResponse>> HandleAsync(RegistrarPresencaRequest request, CancellationToken cancellationToken)
    {
        var palestra = await db.Palestras
            .TagWith("Palestras.RegistrarPresenca.Carregar")
            .Include(p => p.Presencas.Where(x => x.PessoaId == request.PessoaId))
            .FirstOrDefaultAsync(p => p.Id == request.PalestraId, cancellationToken);
        if (palestra is null)
        {
            logger.LogInformation("Registro de presença rejeitado: palestra {TalkId} não encontrada", request.PalestraId);
            return PalestrasErros.PalestraNaoEncontrada;
        }

        logger.LogDebug("Consultando módulo Eventos para validar inscrição da pessoa {PessoaId} no evento {EventoId}", request.PessoaId, palestra.EventoId);
        var inscrito = await eventosApi.InscricaoConfirmadaExisteAsync(palestra.EventoId, request.PessoaId, cancellationToken);
        if (!inscrito)
        {
            logger.LogInformation("Presença rejeitada: pessoa {PessoaId} não possui inscrição confirmada no evento {EventoId}", request.PessoaId, palestra.EventoId);
            return PalestrasErros.ParticipanteNaoInscrito;
        }

        logger.LogDebug("Chamando agregado Palestra {TalkId}.RegistrarPresenca para pessoa {PessoaId}; presenças carregadas={AttendanceCount}", palestra.Id, request.PessoaId, palestra.Presencas.Count);
        var resultado = palestra.RegistrarPresenca(request.PessoaId, timeProvider.GetUtcNow());
        if (resultado.IsFailure)
        {
            logger.LogInformation("Presença da pessoa {PessoaId} na palestra {TalkId} rejeitada pela regra {ErrorCode}", request.PessoaId, palestra.Id, resultado.Error.Code);
            return resultado.Error;
        }

        var presenca = resultado.Value;
        return await db.ExecuteInTransactionAsync(async ct =>
        {
            await db.SaveChangesAsync(ct);
            logger.LogInformation("Presença {AttendanceId} registrada para pessoa {PessoaId} na palestra {TalkId}", presenca.Id, presenca.PessoaId, presenca.PalestraId);
            return Result.Success(new RegistrarPresencaResponse(presenca.Id, presenca.PalestraId, presenca.PessoaId, presenca.PresencaRegistradaEm));
        }, cancellationToken);
    }
}
