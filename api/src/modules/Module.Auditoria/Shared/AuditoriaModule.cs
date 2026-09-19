using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Hosting;
using Module.Auditoria.Shared.Handlers;
using Shared.Contracts.Auditoria;
using Shared.Data;
using Shared.Http.Endpoints;
using Shared.Messaging;
using Shared.WebHost;
using Shared.WebHost.Modules;

namespace Module.Auditoria.Shared;

/// <summary>Ponto de entrada do módulo Auditoria: contexto, handler do evento <c>EntidadeAlterada</c> e endpoints em <c>api/v1/auditoria</c>.</summary>
public sealed class AuditoriaModule : IModule
{
    public string Name => AuditoriaDbContext.SchemaName;
    public string RoutePrefix => "auditoria";
    public string Description => """
        Trilha de auditoria **centralizada e imutável** de todas as alterações de entidades do sistema, consultável apenas por Administradores.

        **Fluxo (assíncrono, sem acoplamento entre módulos):**
        1. O `AuditoriaSaveChangesInterceptor` (Shared.Data) gera um evento `EntidadeAlterada` para cada Insert/Update/Delete (soft) de entidade auditável, com os valores anteriores e novos em JSON.
        2. O evento é gravado na tabela `OutboxMessages` **do schema do módulo de origem**, na mesma transação da alteração de negócio.
        3. O `OutboxProcessor` (Shared.Messaging) lê os Outboxes de todos os módulos e publica os eventos in-process.
        4. O `EntidadeAlteradaHandler` deste módulo persiste o registro na tabela `Auditoria.RegistrosAuditoria`, usando o Id do evento como chave (idempotente diante de reentregas).

        - Registros nunca são alterados nem excluídos; `registradoEm - ocorridoEm` mede a latência do Outbox.
        - Este contexto não gera auditoria de si mesmo (`AuditChangesEnabled = false`).
        - Operações: `Inclusao`, `Alteracao`, `Exclusao`.
        """;

    public void ConfigureServices(IHostApplicationBuilder builder)
    {
        builder.AddModuleDbContext<AuditoriaDbContext>(AuditoriaDbContext.SchemaName);
        builder.Services.AddIntegrationEventHandler<EntidadeAlterada, EntidadeAlteradaHandler>();
        builder.Services.AddUseCasesFromAssembly(typeof(AuditoriaModule).Assembly, AuditoriaTelemetry.Instance);
        builder.Services.AddModuleValidators(typeof(AuditoriaModule).Assembly);
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints) =>
        endpoints.MapModuleGroup(RoutePrefix, Name).MapEndpointsFromAssembly(typeof(AuditoriaModule).Assembly);
}
