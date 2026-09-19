using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Module.Identidade.Domain;
using Shared.Data;
using Shared.Data.Outbox;

namespace Module.Identidade.Shared;

/// <summary>
/// Contexto do módulo Identidade. Herda de <see cref="IdentityDbContext{TUser,TRole,TKey,TUserClaim,TUserRole,TUserLogin,TRoleClaim,TUserToken}"/>
/// (exigência do ASP.NET Core Identity) e, por isso, não pode herdar <c>ModuleDbContext</c>; implementa <see cref="IModuleDbContext"/>
/// e aplica as mesmas convenções (<see cref="ModuleModelConventions"/>): schema "Identidade", Outbox, soft delete em <see cref="Usuario"/>.
/// Tabelas: Usuarios, Perfis, UsuarioPerfis, UsuarioClaims, UsuarioLogins, UsuarioTokens, PerfilClaims, OutboxMessages
/// (passkeys do Identity .NET 10 ficam fora do modelo: <c>Stores.SchemaVersion</c> padrão).
/// </summary>
public sealed class IdentidadeDbContext(DbContextOptions<IdentidadeDbContext> options)
    : IdentityDbContext<Usuario, Perfil, Guid, UsuarioClaim, UsuarioPerfil, UsuarioLogin, PerfilClaim, UsuarioToken>(options), IModuleDbContext
{
    public const string SchemaName = "Identidade";

    public string Schema => SchemaName;
    public bool AuditChangesEnabled => true;
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    /// <summary>Aliases em pt-BR para os DbSets do Identity (Users/Roles/UserRoles).</summary>
    public DbSet<Usuario> Usuarios => Users;
    public DbSet<Perfil> Perfis => Roles;
    public DbSet<UsuarioPerfil> UsuarioPerfis => UserRoles;

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        ModuleModelConventions.ConfigureConventions(configurationBuilder);
        base.ConfigureConventions(configurationBuilder);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema(SchemaName);
        ModuleModelConventions.ConfigureOutbox(builder);

        // Identity configura chaves, índices e relacionamentos; em seguida renomeamos as tabelas e ajustamos colunas.
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(IdentidadeDbContext).Assembly);

        ModuleModelConventions.ConfigureAuditableEntities(builder);
    }
}

/// <summary>
/// Usado apenas por <c>dotnet ef</c> em tempo de design. Equivalente a <c>DesignTimeDbContextFactoryBase</c>, que exige <c>ModuleDbContext</c>.
/// Connection string via env <c>ConnectionStrings__GestaoEventos</c>; caso contrário, o padrão do docker-compose.
/// </summary>
public sealed class IdentidadeDbContextFactory : IDesignTimeDbContextFactory<IdentidadeDbContext>
{
    public IdentidadeDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__GestaoEventos")
            ?? "Host=localhost;Port=5432;Database=gestao_eventos;Username=gestao;Password=gestao";
        var options = new DbContextOptionsBuilder<IdentidadeDbContext>()
            .UseNpgsql(connectionString, npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", IdentidadeDbContext.SchemaName))
            .Options;
        return new IdentidadeDbContext(options);
    }
}
