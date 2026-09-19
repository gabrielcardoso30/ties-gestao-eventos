using Microsoft.EntityFrameworkCore;
using Module.Pessoas.Domain;
using Shared.Data;
using Shared.Data.Migracao;

namespace Module.Pessoas.Shared;

/// <summary>Contexto do módulo Pessoas. Schema "Pessoas"; tabelas Pessoas e OutboxMessages.</summary>
public sealed class PessoasDbContext(DbContextOptions<PessoasDbContext> options) : ModuleDbContext(options)
{
    public const string SchemaName = "Pessoas";
    public override string Schema => SchemaName;

    public DbSet<Pessoa> Pessoas => Set<Pessoa>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PessoasDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}

/// <summary>Usado apenas por <c>dotnet ef</c> em tempo de design.</summary>
public sealed class PessoasDbContextFactory : DesignTimeDbContextFactoryBase<PessoasDbContext>
{
    protected override string Schema => PessoasDbContext.SchemaName;
    protected override PessoasDbContext Create(DbContextOptions<PessoasDbContext> options) => new(options);
}
