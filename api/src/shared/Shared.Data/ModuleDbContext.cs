using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Data.Entidades;
using Shared.Data.Outbox;

namespace Shared.Data;

/// <summary>
/// DbContext base de cada módulo. Garante: schema próprio, nomes PascalCase, soft delete por filtro global nomeado,
/// tabela Outbox no schema do módulo e convenções de tipos (timestamptz, tamanhos padrão).
/// As convenções vivem em <see cref="ModuleModelConventions"/> para reuso por contextos com outra base (ex.: Identity).
/// </summary>
public abstract class ModuleDbContext(DbContextOptions options) : DbContext(options), IModuleDbContext
{
    public const string SoftDeleteFilterName = ModuleModelConventions.SoftDeleteFilterName;

    /// <summary>Nome do schema = nome do módulo (ex.: "Locais").</summary>
    public abstract string Schema { get; }

    /// <summary>Permite ao módulo Auditoria desligar a auto-auditoria e evitar recursão.</summary>
    public virtual bool AuditChangesEnabled => true;

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    /// <summary>Nome do módulo a partir do namespace do contexto (<c>Module.X</c> → "X").</summary>
    public static string ResolveModuleName(Type contextType)
    {
        var ns = contextType.Namespace ?? contextType.Name;
        var partes = ns.Split('.');
        return partes.Length > 1 && partes[0] == "Module" ? partes[1] : contextType.Name.Replace("DbContext", string.Empty, StringComparison.Ordinal);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        ModuleModelConventions.ConfigureConventions(configurationBuilder);
        base.ConfigureConventions(configurationBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        ModuleModelConventions.ConfigureOutbox(modelBuilder);
        base.OnModelCreating(modelBuilder);
        ModuleModelConventions.ConfigureAuditableEntities(modelBuilder);
    }
}

public static class EntityTypeBuilderExtensions
{
    /// <summary>Anotação de propriedade cujo valor nunca deve aparecer na trilha de auditoria (ex.: hash de senha).</summary>
    public const string SensivelAnnotation = "GestaoEventos:Sensivel";

    /// <summary>Configuração padrão de uma entidade principal: tabela PascalCase e PK Guid v7 gerada na aplicação.</summary>
    public static EntityTypeBuilder<TEntity> ConfigurarEntidadeBase<TEntity>(this EntityTypeBuilder<TEntity> builder, string tabela)
        where TEntity : class, IEntidadeAuditavel
    {
        builder.ToTable(tabela);
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.Property(e => e.EstaAtivo).HasDefaultValue(true);
        return builder;
    }

    /// <summary>Marca a propriedade como sensível: o interceptor de auditoria grava <c>"***"</c> no lugar do valor.</summary>
    public static PropertyBuilder<TProperty> Sensivel<TProperty>(this PropertyBuilder<TProperty> builder) =>
        builder.HasAnnotation(SensivelAnnotation, true);
}
