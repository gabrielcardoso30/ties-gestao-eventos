using Shared.WebHost;

var builder = WebApplication.CreateBuilder(args);

// Toda a composição transversal (observabilidade, dados, outbox, segurança, OpenAPI) e a descoberta dos módulos.
builder.AddModularWebHost(serviceName: "GestaoEventos.Api");

var app = builder.Build();

app.UseModularWebHost();

await app.RunAsync();

/// <summary>Exposto para os testes de integração/funcionais (WebApplicationFactory).</summary>
public partial class Program;
