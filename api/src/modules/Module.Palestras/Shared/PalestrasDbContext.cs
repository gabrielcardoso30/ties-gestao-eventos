using Microsoft.EntityFrameworkCore;
using Module.Palestras.Domain;
using Shared.Data;
using Shared.Data.Migracao;

namespace Module.Palestras.Shared;

/// <summary>Contexto do módulo Palestras. Schema "Palestras"; tabelas Palestras, PalestraPalestrantes, PalestraConteudos, Presencas, Certificados, OutboxMessages.</summary>
public sealed class PalestrasDbContext(DbContextOptions<PalestrasDbContext> options) : ModuleDbContext(options)
{
    public const string SchemaName = "Palestras";
    public override string Schema => SchemaName;

    public DbSet<Palestra> Palestras => Set<Palestra>();
    public DbSet<PalestraPalestrante> PalestraPalestrantes => Set<PalestraPalestrante>();
    public DbSet<PalestraConteudo> PalestraConteudos => Set<PalestraConteudo>();
    public DbSet<Presenca> Presencas => Set<Presenca>();
    public DbSet<Certificado> Certificados => Set<Certificado>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PalestrasDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}

/// <summary>Usado apenas por <c>dotnet ef</c> em tempo de design.</summary>
public sealed class PalestrasDbContextFactory : DesignTimeDbContextFactoryBase<PalestrasDbContext>
{
    protected override string Schema => PalestrasDbContext.SchemaName;
    protected override PalestrasDbContext Create(DbContextOptions<PalestrasDbContext> options) => new(options);
}
