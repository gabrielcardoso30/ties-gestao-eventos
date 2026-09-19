using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Data.Entidades;
using Shared.Data.Outbox;

namespace Shared.Data;

/// <summary>
/// DbContext base de cada módulo. Garante: schema próprio, nomes PascalCase, soft delete por filtro global nomeado,
/// tabela Outbox no schema do módulo e convenções de tipos (timestamptz, tamanhos padrão).
/// </summary>
public abstract class ModuleDbContext(DbContextOptions options) : DbContext(options)
{
    public const string SoftDeleteFilterName = "SoftDelete";

    /// <summary>Nome do schema = nome do módulo (ex.: "Locais").</summary>
    public abstract string Schema { get; }

    /// <summary>Permite ao módulo Auditoria desligar a auto-auditoria e evitar recursão.</summary>
    public virtual bool AuditChangesEnabled => true;

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    public static string ResolveModuleName(Type contextType)
    {
        var ns = contextType.Namespace ?? contextType.Name;
        var partes = ns.Split('.');
        return partes.Length > 1 && partes[0] == "Module" ? partes[1] : contextType.Name.Replace("DbContext", string.Empty, StringComparison.Ordinal);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<string>().HaveMaxLength(200);
        configurationBuilder.Properties<decimal>().HavePrecision(18, 2);
        configurationBuilder.Properties<Enum>().HaveConversion<string>().HaveMaxLength(50);
        base.ConfigureConventions(configurationBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);

        modelBuilder.Entity<OutboxMessage>(b =>
        {
            b.ToTable("OutboxMessages");
            b.HasKey(m => m.Id);
            b.Property(m => m.Type).HasMaxLength(500);
            b.Property(m => m.Payload).HasColumnType("jsonb");
            b.Property(m => m.Error).HasMaxLength(2000);
            b.Property(m => m.TraceParent).HasMaxLength(100);
            b.HasIndex(m => new { m.ProcessedOn, m.LockedUntil, m.OccurredOn }).HasDatabaseName("IX_OutboxMessages_Pendentes");
        });

        base.OnModelCreating(modelBuilder);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(IEntidadeAuditavel).IsAssignableFrom(entityType.ClrType) || entityType.BaseType is not null)
            {
                continue;
            }

            var builder = modelBuilder.Entity(entityType.ClrType);
            builder.Property(nameof(IEntidadeAuditavel.CriadoPor)).HasMaxLength(150);
            builder.Property(nameof(IEntidadeAuditavel.AlteradoPor)).HasMaxLength(150);
            builder.Property(nameof(IEntidadeAuditavel.ExcluidoPor)).HasMaxLength(150);
            builder.HasIndex(nameof(IEntidadeAuditavel.ExcluidoEm));
            builder.HasQueryFilter(SoftDeleteFilterName, BuildSoftDeleteFilter(entityType.ClrType));
            builder.Ignore(nameof(IEmissorDeEventos.Eventos));
        }
    }

    private static LambdaExpression BuildSoftDeleteFilter(Type clrType)
    {
        var parameter = Expression.Parameter(clrType, "e");
        var propriedade = Expression.Property(parameter, nameof(IEntidadeAuditavel.ExcluidoEm));
        var body = Expression.Equal(propriedade, Expression.Constant(null, typeof(DateTimeOffset?)));
        return Expression.Lambda(body, parameter);
    }
}

public static class EntityTypeBuilderExtensions
{
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
}
