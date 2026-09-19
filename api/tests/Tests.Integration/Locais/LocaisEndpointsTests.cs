using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Module.Locais.Shared;
using Shared.Contracts.Identidade;
using Shouldly;
using Tests.Integration.Infra;
using Xunit;

namespace Tests.Integration.Locais;

[Collection(ApiCollection.Nome)]
public sealed class LocaisEndpointsTests(ApiFactory factory)
{
    private static object NovoLocal(string nome, int? capacidade = 120) => new
    {
        localNome = nome,
        localDescricao = "Criado em teste de integração",
        enderecoCidade = "Vila Velha",
        enderecoUf = "es",
        capacidadeAmbienteUnico = capacidade,
    };

    [Fact]
    public async Task Sem_token_deve_retornar_401_problem_details()
    {
        var client = factory.CreateClient();
        var response = await client.GetAsync("/api/v1/locais");
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Participante_nao_pode_criar_local_403()
    {
        var client = factory.ClienteAutenticado(PerfisPadrao.Participante);
        var response = await client.PostAsJsonAsync("/api/v1/locais", NovoLocal($"Local {Guid.NewGuid():N}"));
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Criar_local_com_ambiente_unico_gera_uma_sala_e_registra_auditoria()
    {
        var client = factory.ClienteAutenticado();
        var nome = $"Auditório {Guid.NewGuid():N}";
        var inicio = DateTimeOffset.UtcNow.AddSeconds(-5);

        var criado = await client.PostAsJsonAsync("/api/v1/locais", NovoLocal(nome));
        criado.StatusCode.ShouldBe(HttpStatusCode.Created);
        criado.Headers.Location.ShouldNotBeNull();
        criado.Headers.GetValues("X-Correlation-Id").ShouldNotBeEmpty();
        var corpo = await criado.Content.ReadFromJsonAsync<CriadoResponse>();
        corpo!.SalasQuantidade.ShouldBe(1);

        var detalhe = await client.GetFromJsonAsync<LocalDetalhe>(criado.Headers.Location);
        detalhe!.LocalNome.ShouldBe(nome);
        detalhe.EnderecoUf.ShouldBe("ES");
        detalhe.Salas.Count.ShouldBe(1);
        detalhe.Salas[0].SalaTipo.ShouldBe("AmbienteUnico");
        detalhe.LocalCapacidadeTotal.ShouldBe(120);

        // Campos de auditoria preenchidos pelo interceptor e Outbox gravado na mesma transação
        var (criadoPor, mensagens) = await factory.ComServicoAsync(async sp =>
        {
            var db = sp.GetRequiredService<LocaisDbContext>();
            var local = await db.Locais.TagWith("Testes.Locais.Auditoria").AsNoTracking().FirstAsync(l => l.Id == corpo.Id);
            // Payload é jsonb: filtra por período no banco e pelo id em memória (tabela pequena no teste).
            var recentes = await db.OutboxMessages.TagWith("Testes.Locais.Outbox").AsNoTracking()
                .Where(m => m.OccurredOn >= inicio).Select(m => new { m.Type, m.Payload }).ToListAsync();
            var msgs = recentes.Where(m => m.Payload.Contains(corpo.Id.ToString(), StringComparison.OrdinalIgnoreCase)).Select(m => m.Type).ToList();
            return (local.CriadoPor, msgs);
        });
        criadoPor.ShouldBe("Usuário de Teste");
        mensagens.ShouldContain("Shared.Contracts.Locais.LocalCriado");
        mensagens.ShouldContain("Shared.Contracts.Auditoria.EntidadeAlterada");
    }

    [Fact]
    public async Task Nome_duplicado_deve_retornar_409_com_codigo()
    {
        var client = factory.ClienteAutenticado();
        var nome = $"Duplicado {Guid.NewGuid():N}";
        (await client.PostAsJsonAsync("/api/v1/locais", NovoLocal(nome))).EnsureSuccessStatusCode();

        var repetido = await client.PostAsJsonAsync("/api/v1/locais", NovoLocal(nome));
        repetido.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        var problem = await ProblemDetailsDeTeste.LerAsync(repetido);
        problem.Codigo.ShouldBe("Locais.LocalNomeDuplicado");
        problem.TraceId.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Payload_invalido_deve_retornar_400_com_erros_por_campo()
    {
        var client = factory.ClienteAutenticado();
        var response = await client.PostAsJsonAsync("/api/v1/locais", new { localNome = "", enderecoCidade = "", enderecoUf = "ESP" });
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem = await ProblemDetailsDeTeste.LerAsync(response);
        problem.Codigo.ShouldBe("Validacao");
        problem.Errors!.Keys.ShouldContain("LocalNome");
        problem.Errors.Keys.ShouldContain("EnderecoUf");
    }

    [Fact]
    public async Task Nao_deve_remover_a_ultima_sala_422_e_soft_delete_da_sala_extra()
    {
        var client = factory.ClienteAutenticado();
        var criado = await client.PostAsJsonAsync("/api/v1/locais", NovoLocal($"Salas {Guid.NewGuid():N}"));
        var local = (await criado.Content.ReadFromJsonAsync<CriadoResponse>())!;

        var sala = await client.PostAsJsonAsync($"/api/v1/locais/{local.Id}/salas", new { salaNome = "Lab 1", salaCapacidade = 30, salaTipo = "Laboratorio" });
        sala.StatusCode.ShouldBe(HttpStatusCode.Created);
        var salaCriada = (await sala.Content.ReadFromJsonAsync<SalaResponse>())!;

        (await client.DeleteAsync($"/api/v1/locais/{local.Id}/salas/{salaCriada.Id}")).StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var detalhe = await client.GetFromJsonAsync<LocalDetalhe>($"/api/v1/locais/{local.Id}");
        detalhe!.Salas.Count.ShouldBe(1);
        var ultima = await client.DeleteAsync($"/api/v1/locais/{local.Id}/salas/{detalhe.Salas[0].Id}");
        ultima.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        (await ProblemDetailsDeTeste.LerAsync(ultima)).Codigo.ShouldBe("Locais.LocalPrecisaDeUmaSala");

        var excluida = await factory.ComServicoAsync(async sp =>
        {
            var db = sp.GetRequiredService<LocaisDbContext>();
            return await db.Salas.TagWith("Testes.Locais.SoftDelete").IgnoreQueryFilters([Shared.Data.ModuleDbContext.SoftDeleteFilterName])
                .AsNoTracking().Where(s => s.Id == salaCriada.Id).Select(s => new { s.ExcluidoEm, s.ExcluidoPor, s.EstaAtivo }).FirstAsync();
        });
        excluida.ExcluidoEm.ShouldNotBeNull();
        excluida.ExcluidoPor.ShouldBe("Usuário de Teste");
        excluida.EstaAtivo.ShouldBeFalse();
    }

    [Fact]
    public async Task Listagem_deve_ser_paginada_e_filtrar_por_busca()
    {
        var client = factory.ClienteAutenticado();
        var marcador = Guid.NewGuid().ToString("N")[..8];
        for (var i = 0; i < 3; i++)
        {
            (await client.PostAsJsonAsync("/api/v1/locais", NovoLocal($"Pag {marcador} {i}"))).EnsureSuccessStatusCode();
        }

        var pagina = await client.GetFromJsonAsync<Paginado<LocalItem>>($"/api/v1/locais?busca={marcador}&pagina=1&tamanhoPagina=2");
        pagina!.Total.ShouldBe(3);
        pagina.Itens.Count.ShouldBe(2);
        pagina.TotalPaginas.ShouldBe(2);
    }

    [Fact]
    public async Task OpenApi_deve_expor_uma_unica_tag_por_modulo_e_yaml()
    {
        var client = factory.CreateClient();
        var json = await client.GetStringAsync("/openapi/v1.json");
        json.ShouldContain("\"Locais\"");
        var yaml = await client.GetStringAsync("/openapi/v1.yaml");
        yaml.ShouldStartWith("openapi:");
    }

    private sealed record CriadoResponse(Guid Id, string LocalNome, int SalasQuantidade);
    private sealed record SalaResponse(Guid Id, Guid LocalId, string SalaNome);
    private sealed record SalaDetalhe(Guid Id, string SalaNome, int SalaCapacidade, string SalaTipo, bool EstaAtivo);
    private sealed record LocalDetalhe(Guid Id, string LocalNome, string EnderecoUf, int LocalCapacidadeTotal, List<SalaDetalhe> Salas);
    private sealed record LocalItem(Guid Id, string LocalNome);
    private sealed record Paginado<T>(List<T> Itens, int Pagina, int TamanhoPagina, long Total, int TotalPaginas);
}
