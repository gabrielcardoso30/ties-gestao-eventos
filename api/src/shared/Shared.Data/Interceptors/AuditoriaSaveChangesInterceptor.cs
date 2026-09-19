using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Shared.Contracts.Auditoria;
using Shared.Contracts.Common;
using Shared.Contracts.Integracao;
using Shared.Data.Entidades;
using Shared.Data.Outbox;

namespace Shared.Data.Interceptors;

/// <summary>
/// Um único ponto que garante três regras transversais do projeto, na MESMA transação do negócio:
/// 1) preenchimento dos campos de auditoria (CriadoEm/Por, AlteradoEm/Por);
/// 2) soft delete (Delete vira Update com ExcluidoEm/Por e EstaAtivo=false);
/// 3) Outbox: eventos de integração das entidades + um <see cref="EntidadeAlterada"/> por alteração (trilha de auditoria).
/// </summary>
public sealed class AuditoriaSaveChangesInterceptor(ICurrentUser currentUser, TimeProvider timeProvider) : SaveChangesInterceptor
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public const string UsuarioSistema = "sistema";

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        if (eventData.Context is IModuleDbContext contexto)
        {
            Processar(eventData.Context, contexto);
        }

        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        if (eventData.Context is IModuleDbContext contexto)
        {
            Processar(eventData.Context, contexto);
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void Processar(DbContext db, IModuleDbContext contexto)
    {
        var agora = timeProvider.GetUtcNow();
        var usuarioNome = currentUser.Nome ?? currentUser.Email ?? UsuarioSistema;
        var modulo = ModuleDbContext.ResolveModuleName(db.GetType());
        var traceId = Activity.Current?.TraceId.ToString();
        var traceParent = Activity.Current?.Id;
        var mensagens = new List<OutboxMessage>();

        db.ChangeTracker.DetectChanges();
        var entries = db.ChangeTracker.Entries().Where(e => e.Entity is not OutboxMessage).ToList();

        foreach (var entry in entries)
        {
            if (entry.Entity is IEntidadeAuditavel auditavel)
            {
                AplicarAuditoria(entry, auditavel, agora, usuarioNome);
            }

            if (contexto.AuditChangesEnabled && entry.State is EntityState.Added or EntityState.Modified)
            {
                var registro = CriarRegistroAuditoria(entry, modulo, traceId);
                if (registro is not null)
                {
                    mensagens.Add(ParaOutbox(registro, agora, traceParent));
                }
            }
        }

        foreach (var emissor in db.ChangeTracker.Entries<IEmissorDeEventos>().Select(e => e.Entity).Where(e => e.Eventos.Count > 0).ToList())
        {
            mensagens.AddRange(emissor.Eventos.Select(ev => ParaOutbox(ev, agora, traceParent)));
            emissor.LimparEventos();
        }

        if (mensagens.Count > 0)
        {
            contexto.OutboxMessages.AddRange(mensagens);
        }
    }

    private static void AplicarAuditoria(EntityEntry entry, IEntidadeAuditavel entidade, DateTimeOffset agora, string usuario)
    {
        switch (entry.State)
        {
            case EntityState.Added:
                entidade.CriadoEm = agora;
                entidade.CriadoPor = usuario;
                break;
            case EntityState.Modified:
                entry.Property(nameof(IEntidadeAuditavel.CriadoEm)).IsModified = false;
                entry.Property(nameof(IEntidadeAuditavel.CriadoPor)).IsModified = false;
                entidade.AlteradoEm = agora;
                entidade.AlteradoPor = usuario;
                break;
            case EntityState.Deleted:
                entry.State = EntityState.Modified;
                entidade.ExcluidoEm = agora;
                entidade.ExcluidoPor = usuario;
                entidade.EstaAtivo = false;
                break;
        }
    }

    private EntidadeAlterada? CriarRegistroAuditoria(EntityEntry entry, string modulo, string? traceId)
    {
        var chave = entry.Properties.Where(p => p.Metadata.IsPrimaryKey()).Select(p => p.CurrentValue?.ToString()).FirstOrDefault() ?? string.Empty;
        var nome = entry.Metadata.ClrType.Name;

        if (entry.State == EntityState.Added)
        {
            var novos = entry.Properties.ToDictionary(p => p.Metadata.Name, p => ValorAuditavel(p, p.CurrentValue));
            return Novo(modulo, nome, chave, OperacoesAuditoria.Inclusao, null, novos, traceId);
        }

        var alteradas = entry.Properties.Where(p => p.IsModified).ToList();
        if (alteradas.Count == 0)
        {
            return null;
        }

        var excluindo = alteradas.Any(p => p.Metadata.Name == nameof(IEntidadeAuditavel.ExcluidoEm) && p.CurrentValue is not null);
        var anteriores = alteradas.ToDictionary(p => p.Metadata.Name, p => ValorAuditavel(p, p.OriginalValue));
        var atuais = alteradas.ToDictionary(p => p.Metadata.Name, p => ValorAuditavel(p, p.CurrentValue));
        return Novo(modulo, nome, chave, excluindo ? OperacoesAuditoria.Exclusao : OperacoesAuditoria.Alteracao, anteriores, atuais, traceId);
    }

    /// <summary>Propriedades marcadas com <see cref="EntityTypeBuilderExtensions.Sensivel{TProperty}"/> nunca vão para a trilha (ex.: hash de senha).</summary>
    private static object? ValorAuditavel(PropertyEntry propriedade, object? valor) =>
        valor is not null && propriedade.Metadata.FindAnnotation(EntityTypeBuilderExtensions.SensivelAnnotation)?.Value is true ? "***" : valor;

    private EntidadeAlterada Novo(string modulo, string entidade, string chave, string operacao, Dictionary<string, object?>? antes, Dictionary<string, object?> depois, string? traceId) =>
        new(modulo, entidade, chave, operacao,
            antes is null ? null : JsonSerializer.Serialize(antes, JsonOptions),
            JsonSerializer.Serialize(depois, JsonOptions),
            currentUser.Id, currentUser.Nome ?? currentUser.Email ?? UsuarioSistema, traceId);

    private static OutboxMessage ParaOutbox(IIntegrationEvent integrationEvent, DateTimeOffset agora, string? traceParent)
    {
        var tipo = integrationEvent.GetType();
        return new OutboxMessage
        {
            Type = tipo.FullName ?? tipo.Name,
            Payload = JsonSerializer.Serialize(integrationEvent, tipo, JsonOptions),
            OccurredOn = agora,
            TraceParent = traceParent,
        };
    }
}
