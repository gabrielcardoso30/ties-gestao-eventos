using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Shared.Data.Interceptors;
using Shared.Data.Migracao;
using Shared.Data.Outbox;

namespace Shared.Data;

public static class DataServiceCollectionExtensions
{
    public const string ConnectionStringName = "GestaoEventos";

    /// <summary>Infraestrutura de dados compartilhada (interceptors, registro de contextos, migrador). Chamado uma vez pelo host.</summary>
    public static IHostApplicationBuilder AddSharedData(this IHostApplicationBuilder builder)
    {
        builder.Services.TryAddSingleton(TimeProvider.System);
        builder.Services.TryAddSingleton<AuditoriaSaveChangesInterceptor>();
        builder.Services.TryAddSingleton<QueryTagInterceptor>();
        builder.Services.TryAddSingleton<ModuleDbContextRegistry>();
        builder.Services.AddHostedService<DatabaseMigrationHostedService>();
        return builder;
    }

    /// <summary>
    /// Registra o DbContext de um módulo: pool de contextos (performance), schema próprio, histórico de migrações no schema,
    /// retry para falhas transitórias, interceptors de auditoria/outbox e a fonte Outbox do módulo.
    /// </summary>
    public static IHostApplicationBuilder AddModuleDbContext<TContext>(this IHostApplicationBuilder builder, string schema)
        where TContext : ModuleDbContext
    {
        var connectionString = builder.Configuration.GetConnectionString(ConnectionStringName)
            ?? throw new InvalidOperationException($"ConnectionStrings:{ConnectionStringName} não configurada");

        builder.Services.AddDbContextPool<TContext>((sp, options) =>
        {
            options.UseNpgsql(connectionString, npgsql =>
            {
                npgsql.MigrationsHistoryTable("__EFMigrationsHistory", schema);
                npgsql.MigrationsAssembly(typeof(TContext).Assembly.GetName().Name);
                npgsql.EnableRetryOnFailure(maxRetryCount: 3);
                npgsql.CommandTimeout(30);
            });
            options.AddInterceptors(sp.GetRequiredService<AuditoriaSaveChangesInterceptor>(), sp.GetRequiredService<QueryTagInterceptor>());
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll);
            if (builder.Environment.IsDevelopment())
            {
                options.EnableDetailedErrors();
            }
        });

        builder.Services.AddSingleton<IOutboxStore, OutboxStore<TContext>>();

        var registry = builder.Services.BuildRegistryPlaceholder();
        registry.Add(typeof(TContext), schema);
        return builder;
    }

    private static ModuleDbContextRegistry BuildRegistryPlaceholder(this IServiceCollection services)
    {
        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(ModuleDbContextRegistry));
        if (descriptor?.ImplementationInstance is ModuleDbContextRegistry existente)
        {
            return existente;
        }

        var registry = new ModuleDbContextRegistry();
        services.Replace(ServiceDescriptor.Singleton(registry));
        return registry;
    }
}
