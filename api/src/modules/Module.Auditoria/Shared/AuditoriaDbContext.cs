using Microsoft.EntityFrameworkCore;
using Module.Auditoria.Domain;
using Shared.Data;
using Shared.Data.Migracao;

namespace Module.Auditoria.Shared;

/// <summary>
/// Contexto do módulo Auditoria. Schema "Auditoria"; tabela RegistrosAuditoria (append-only).
/// <see cref="AuditChangesEnabled"/> é falso: gravar um registro de auditoria não gera outro registro de auditoria.
/// </summary>
public sealed class AuditoriaDbContext(DbContextOptions<AuditoriaDbContext> options) : ModuleDbContext(options)
{
    public const string SchemaName = "Auditoria";
    public override string Schema => SchemaName;
    public override bool AuditChangesEnabled => false;

    public DbSet<RegistroAuditoria> RegistrosAuditoria => Set<RegistroAuditoria>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuditoriaDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}

/// <summary>Usado apenas por <c>dotnet ef</c> em tempo de design.</summary>
public sealed class AuditoriaDbContextFactory : DesignTimeDbContextFactoryBase<AuditoriaDbContext>
{
    protected override string Schema => AuditoriaDbContext.SchemaName;
    protected override AuditoriaDbContext Create(DbContextOptions<AuditoriaDbContext> options) => new(options);
}
