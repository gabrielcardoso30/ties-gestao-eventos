using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared.Contracts.Integracao;

namespace Shared.Messaging;

/// <summary>
/// Publicador in-process: resolve <c>IIntegrationEventHandler&lt;T&gt;</c> em um escopo DI próprio por handler,
/// com span de trace por entrega. Falha em um handler não impede os demais, mas marca a mensagem para retry.
/// </summary>
internal sealed class InProcessIntegrationEventPublisher(IServiceScopeFactory scopeFactory, ILogger<InProcessIntegrationEventPublisher> logger) : IIntegrationEventPublisher
{
    private static readonly ActivitySource ActivitySource = new("GestaoEventos.Messaging");
    private static readonly ConcurrentDictionary<Type, MethodInfo> HandleMethods = new();

    public async Task PublishAsync(IIntegrationEvent integrationEvent, CancellationToken cancellationToken)
    {
        var tipoEvento = integrationEvent.GetType();
        var tipoHandler = typeof(IIntegrationEventHandler<>).MakeGenericType(tipoEvento);

        await using var scope = scopeFactory.CreateAsyncScope();
        var handlers = scope.ServiceProvider.GetServices(tipoHandler).Where(h => h is not null).ToList();
        if (handlers.Count == 0)
        {
            logger.LogDebug("Nenhum handler registrado para {IntegrationEvent}", tipoEvento.Name);
            return;
        }

        var falhas = new List<Exception>();
        foreach (var handler in handlers)
        {
            using var activity = ActivitySource.StartActivity($"consume {tipoEvento.Name}", ActivityKind.Consumer);
            activity?.SetTag("messaging.event.type", tipoEvento.FullName);
            activity?.SetTag("messaging.event.id", integrationEvent.Id);
            activity?.SetTag("messaging.handler", handler!.GetType().FullName);
            try
            {
                var method = HandleMethods.GetOrAdd(tipoHandler, t => t.GetMethod(nameof(IIntegrationEventHandler<IIntegrationEvent>.HandleAsync))!);
                await (Task)method.Invoke(handler, [integrationEvent, cancellationToken])!;
            }
            catch (Exception ex)
            {
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                logger.LogError(ex, "Falha no handler {Handler} para {IntegrationEvent} {EventId}", handler!.GetType().Name, tipoEvento.Name, integrationEvent.Id);
                falhas.Add(ex);
            }
        }

        if (falhas.Count > 0)
        {
            throw new AggregateException($"Falha em {falhas.Count} handler(s) de {tipoEvento.Name}", falhas);
        }
    }
}
