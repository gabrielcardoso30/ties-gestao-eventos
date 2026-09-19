using Microsoft.Extensions.Logging;
using Module.Eventos.Domain;
using Module.Eventos.Shared;
using Shared.Contracts.Locais;
using Shared.Data.Extensions;
using Shared.Http.Endpoints;
using Shared.Http.Results;

namespace Module.Eventos.UseCases.CriarEvento;

internal sealed class CriarEventoUseCase(EventosDbContext db, ILocaisModuleApi locais, ILogger<CriarEventoUseCase> logger) : IUseCase<CriarEventoRequest, CriarEventoResponse>
{
    public async Task<Result<CriarEventoResponse>> HandleAsync(CriarEventoRequest request, CancellationToken cancellationToken)
    {
        if (request.LocalId.HasValue)
        {
            logger.LogDebug("Consultando módulo Locais para validar o local {LocalId} do novo evento", request.LocalId);
            var local = await locais.ObterLocalResumoAsync(request.LocalId.Value, cancellationToken);
            if (local is null)
            {
                logger.LogInformation("Criação do evento impedida porque o local {LocalId} não existe", request.LocalId);
                return EventosErros.LocalNaoEncontrado;
            }
        }
        else
        {
            logger.LogDebug("Novo evento não referencia local; validação no módulo Locais não é necessária");
        }

        logger.LogDebug("Chamando fábrica de domínio Evento.Criar com formato {EventoFormato}", request.EventoFormato);
        var resultado = Evento.Criar(
            request.EventoNome, request.EventoDescricao, request.EventoDataInicio, request.EventoDataFim,
            request.EventoFormato, request.LocalId, request.EventoLinkRemoto, request.EventoCapacidadeMaxima);
        if (resultado.IsFailure)
        {
            logger.LogInformation("Agregado rejeitou a criação do evento pela regra {ErrorCode}", resultado.Error.Code);
            return resultado.Error;
        }

        var evento = resultado.Value;
        var trilhas = request.Trilhas is { Count: > 0 }
            ? request.Trilhas
            : [new CriarEventoTrilhaRequest("Trilha única", null, "#2563EB")];
        logger.LogInformation("Montando evento {EventoId} com {TrackCount} trilha(s); trilha padrão aplicada={DefaultTrackApplied}",
            evento.Id, trilhas.Count, request.Trilhas is not { Count: > 0 });
        foreach (var item in trilhas)
        {
            logger.LogDebug("Adicionando trilha {TrackIndex} de {TrackCount} ao evento {EventoId}", evento.Trilhas.Count + 1, trilhas.Count, evento.Id);
            var trilhaResultado = evento.AdicionarTrilha(item.TrilhaNome, item.TrilhaDescricao, item.TrilhaCor);
            if (trilhaResultado.IsFailure)
            {
                logger.LogInformation("Agregado rejeitou uma trilha do evento {EventoId} pela regra {ErrorCode}", evento.Id, trilhaResultado.Error.Code);
                return trilhaResultado.Error;
            }
            logger.LogDebug("Trilha {TrackId} adicionada em memória ao evento {EventoId}", trilhaResultado.Value.Id, evento.Id);
        }
        return await db.ExecuteInTransactionAsync(async ct =>
        {
            db.Eventos.Add(evento);
            await db.SaveChangesAsync(ct);
            logger.LogInformation("Evento {EventoId} persistido em situação {EventoSituacao} com {TrackCount} trilha(s)", evento.Id, evento.EventoSituacao, evento.Trilhas.Count);
            return Result.Success(new CriarEventoResponse(evento.Id, evento.EventoNome, evento.EventoSituacao));
        }, cancellationToken);
    }
}
