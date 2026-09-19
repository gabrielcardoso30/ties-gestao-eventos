using Microsoft.EntityFrameworkCore;
using Module.Locais.Domain;
using Shared.Data;
using Shared.Data.Migracao;

namespace Module.Locais.Shared;

/// <summary>Contexto do módulo Locais. Schema "Locais"; tabelas Locais, Salas, OutboxMessages.</summary>
public sealed class LocaisDbContext(DbContextOptions<LocaisDbContext> options) : ModuleDbContext(options)
{
    public const string SchemaName = "Locais";
    public override string Schema => SchemaName;

    public DbSet<Local> Locais => Set<Local>();
    public DbSet<Sala> Salas => Set<Sala>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LocaisDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}

/// <summary>Usado apenas por <c>dotnet ef</c> em tempo de design.</summary>
public sealed class LocaisDbContextFactory : DesignTimeDbContextFactoryBase<LocaisDbContext>
{
    protected override string Schema => LocaisDbContext.SchemaName;
    protected override LocaisDbContext Create(DbContextOptions<LocaisDbContext> options) => new(options);
}
