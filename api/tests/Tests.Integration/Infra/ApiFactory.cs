using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Xunit;

namespace Tests.Integration.Infra;

/// <summary>
/// Sobe a API completa (todos os módulos descobertos) contra um PostgreSQL real em container (Testcontainers).
/// As migrações rodam na subida, como em produção. Compartilhado entre as classes de teste da coleção "Api".
/// </summary>
public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string JwtSigningKey = "chave-de-testes-integrados-nao-usar-em-producao-0123456789abcdef";

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:17-alpine")
        .WithDatabase("gestao_eventos_testes")
        .WithUsername("gestao")
        .WithPassword("gestao")
        .Build();

    public string ConnectionString => _postgres.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.UseSetting("ConnectionStrings:GestaoEventos", ConnectionString);
        builder.UseSetting("Database:MigrateOnStartup", "true");
        builder.UseSetting("Jwt:SigningKey", JwtSigningKey);
        builder.UseSetting("Outbox:PollingIntervalMs", "200");
        builder.UseSetting("RateLimiting:PermitLimit", "100000");
        builder.UseSetting("Serilog:MinimumLevel:Default", "Warning");
        builder.UseSetting("Identidade:AdministradorInicial:Senha", "Admin@123456");
    }

    /// <summary>Executa uma ação com um serviço scoped (ex.: DbContext de um módulo) para verificar efeitos no banco.</summary>
    public async Task<T> ComServicoAsync<T>(Func<IServiceProvider, Task<T>> acao)
    {
        await using var scope = Services.CreateAsyncScope();
        return await acao(scope.ServiceProvider);
    }
}

[CollectionDefinition(Nome)]
public sealed class ApiCollection : ICollectionFixture<ApiFactory>
{
    public const string Nome = "Api";
}
