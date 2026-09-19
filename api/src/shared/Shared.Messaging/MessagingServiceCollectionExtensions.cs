using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shared.Contracts.Integracao;

namespace Shared.Messaging;

public static class MessagingServiceCollectionExtensions
{
    /// <summary>Publicador in-process + processador do Outbox. Registre no host que deve consumir o Outbox (Api ou Worker).</summary>
    public static IHostApplicationBuilder AddSharedMessaging(this IHostApplicationBuilder builder)
    {
        builder.Services.Configure<OutboxOptions>(builder.Configuration.GetSection(OutboxOptions.Secao));
        builder.Services.AddSingleton<IntegrationEventTypeRegistry>();
        builder.Services.AddSingleton<IIntegrationEventPublisher, InProcessIntegrationEventPublisher>();
        builder.Services.AddHostedService<OutboxProcessor>();
        return builder;
    }

    /// <summary>Registra um handler de evento de integração (scoped).</summary>
    public static IServiceCollection AddIntegrationEventHandler<TEvent, THandler>(this IServiceCollection services)
        where TEvent : IIntegrationEvent
        where THandler : class, IIntegrationEventHandler<TEvent>
    {
        services.AddScoped<IIntegrationEventHandler<TEvent>, THandler>();
        return services;
    }
}
