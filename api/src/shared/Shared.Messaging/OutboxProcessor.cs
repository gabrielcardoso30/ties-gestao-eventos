using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Contracts.Integracao;
using Shared.Data.Outbox;

namespace Shared.Messaging;

/// <summary>
/// Processa o Outbox de todos os módulos. Roda no Host.Api hoje; pode ser movido para um Host.Worker sem mudar nada
/// nos módulos (basta registrar <c>AddSharedMessaging</c> lá). Retry com backoff exponencial e trace vinculado ao original.
/// </summary>
internal sealed class OutboxProcessor(
    IEnumerable<IOutboxStore> stores,
    IIntegrationEventPublisher publisher,
    IntegrationEventTypeRegistry registry,
    IOptions<OutboxOptions> options,
    ILogger<OutboxProcessor> logger) : BackgroundService
{
    private static readonly ActivitySource ActivitySource = new("GestaoEventos.Messaging");
    private static readonly Meter Meter = new("GestaoEventos.Messaging");
    private static readonly Counter<long> Processadas = Meter.CreateCounter<long>("outbox.messages.processed");
    private static readonly Counter<long> Falhas = Meter.CreateCounter<long>("outbox.messages.failed");
    private static readonly Histogram<double> Latencia = Meter.CreateHistogram<double>("outbox.delivery.latency", unit: "ms", description: "Tempo entre gravação e entrega");
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var cfg = options.Value;
        if (!cfg.Enabled)
        {
            logger.LogInformation("Outbox desabilitado por configuração");
            return;
        }

        logger.LogInformation("Outbox iniciado para {Modulos}", string.Join(", ", stores.Select(s => s.Modulo)));
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(cfg.PollingIntervalMs));
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var processou = false;
                foreach (var store in stores)
                {
                    processou |= await ProcessarStoreAsync(store, cfg, stoppingToken) > 0;
                }

                if (!processou)
                {
                    await timer.WaitForNextTickAsync(stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro inesperado no ciclo do Outbox");
                await timer.WaitForNextTickAsync(stoppingToken);
            }
        }
    }

    private async Task<int> ProcessarStoreAsync(IOutboxStore store, OutboxOptions cfg, CancellationToken ct)
    {
        var mensagens = await store.ClaimBatchAsync(cfg.BatchSize, TimeSpan.FromSeconds(cfg.LockSeconds), ct);
        foreach (var mensagem in mensagens)
        {
            var parent = ActivityContext.TryParse(mensagem.TraceParent, null, out var ctx) ? ctx : default;
            using var activity = ActivitySource.StartActivity($"outbox process {mensagem.Type.Split('.')[^1]}", ActivityKind.Internal, parent);
            activity?.SetTag("messaging.module", store.Modulo);
            activity?.SetTag("messaging.message.id", mensagem.Id);
            activity?.SetTag("messaging.attempts", mensagem.Attempts);

            var tags = new TagList { { "module", store.Modulo }, { "event.type", mensagem.Type.Split('.')[^1] } };
            try
            {
                var tipo = registry.Resolve(mensagem.Type) ?? throw new InvalidOperationException($"Tipo de evento desconhecido: {mensagem.Type}");
                var evento = (IIntegrationEvent)JsonSerializer.Deserialize(mensagem.Payload, tipo, JsonOptions)!;
                await publisher.PublishAsync(evento, ct);
                await store.MarkProcessedAsync(mensagem.Id, ct);
                Processadas.Add(1, tags);
                Latencia.Record((DateTimeOffset.UtcNow - mensagem.OccurredOn).TotalMilliseconds, tags);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                Falhas.Add(1, tags);
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                var tentativa = mensagem.Attempts + 1;
                var atraso = tentativa >= cfg.MaxAttempts ? TimeSpan.FromDays(365) : TimeSpan.FromSeconds(Math.Min(300, Math.Pow(2, tentativa)));
                logger.LogError(ex, "Falha ao processar mensagem {MessageId} ({Type}) do módulo {Modulo}, tentativa {Tentativa}", mensagem.Id, mensagem.Type, store.Modulo, tentativa);
                await store.MarkFailedAsync(mensagem.Id, ex.ToString(), atraso, ct);
            }
        }

        return mensagens.Count;
    }
}
