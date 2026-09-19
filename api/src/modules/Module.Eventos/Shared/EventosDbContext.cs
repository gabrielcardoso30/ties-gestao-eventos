using Microsoft.EntityFrameworkCore;
using Module.Eventos.Domain;
using Shared.Data;
using Shared.Data.Migracao;

namespace Module.Eventos.Shared;

/// <summary>Contexto do módulo Eventos. Schema "Eventos"; tabelas Eventos, Inscricoes, OutboxMessages.</summary>
public sealed class EventosDbContext(DbContextOptions<EventosDbContext> options) : ModuleDbContext(options)
{
    public const string SchemaName = "Eventos";
    public override string Schema => SchemaName;

    public DbSet<Evento> Eventos => Set<Evento>();
    public DbSet<Inscricao> Inscricoes => Set<Inscricao>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EventosDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}

/// <summary>Usado apenas por <c>dotnet ef</c> em tempo de design.</summary>
public sealed class EventosDbContextFactory : DesignTimeDbContextFactoryBase<EventosDbContext>
{
    protected override string Schema => EventosDbContext.SchemaName;
    protected override EventosDbContext Create(DbContextOptions<EventosDbContext> options) => new(options);
}
