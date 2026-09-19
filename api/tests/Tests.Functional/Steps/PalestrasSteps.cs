using System.Net;
using Reqnroll;
using Shouldly;

namespace Tests.Functional.Steps;

[Binding]
public sealed class PalestrasSteps(ContextoDoCenario contexto)
{
    private DateTimeOffset Inicio => DateTimeOffset.UtcNow.AddDays(10);

    [Given("que existem evento, sala e palestrante para a palestra")]
    public async Task PrepararDependencias()
    {
        var local = await contexto.PostAsync("/api/v1/locais", new { localNome = contexto.NomeUnico("Local Palestra"), enderecoCidade = "Vila Velha", enderecoUf = "ES", capacidadeAmbienteUnico = 100 });
        local.EnsureSuccessStatusCode();
        contexto.Ids["local"] = (await contexto.CorpoJsonAsync()).GetProperty("id").GetGuid();
        await contexto.GetAsync($"/api/v1/locais/{contexto.Ids["local"]}");
        contexto.Ids["sala"] = (await contexto.CorpoJsonAsync()).GetProperty("salas")[0].GetProperty("id").GetGuid();

        var pessoa = await contexto.PostAsync("/api/v1/pessoas", new { pessoaNome = contexto.NomeUnico("Palestrante"), pessoaEmail = $"palestrante-{contexto.Sufixo}@teste.local" });
        pessoa.EnsureSuccessStatusCode();
        contexto.Ids["pessoa"] = (await contexto.CorpoJsonAsync()).GetProperty("id").GetGuid();

        var evento = await contexto.PostAsync("/api/v1/eventos", new { eventoNome = contexto.NomeUnico("Evento Palestra"), eventoDataInicio = Inicio, eventoDataFim = Inicio.AddHours(8), eventoFormato = "Presencial", localId = contexto.Ids["local"] });
        evento.EnsureSuccessStatusCode();
        contexto.Ids["evento"] = (await contexto.CorpoJsonAsync()).GetProperty("id").GetGuid();
        await contexto.GetAsync($"/api/v1/eventos/{contexto.Ids["evento"]}");
        contexto.Ids["trilha"] = (await contexto.CorpoJsonAsync()).GetProperty("trilhas")[0].GetProperty("id").GetGuid();
    }

    [Given("que existe uma palestra cadastrada")]
    [When("crio uma palestra válida")]
    public async Task CriarPalestra()
    {
        var response = await contexto.PostAsync("/api/v1/palestras", Payload(new[] { new { pessoaId = contexto.Ids["pessoa"], palestrantePapel = "Principal" } }));
        if (response.StatusCode == HttpStatusCode.Created)
            contexto.Ids["palestra"] = (await contexto.CorpoJsonAsync()).GetProperty("id").GetGuid();
    }

    [When("tento criar uma palestra sem palestrantes")]
    public async Task CriarSemPalestrantes() => await contexto.PostAsync("/api/v1/palestras", Payload(Array.Empty<object>()));

    [When("adiciono o conteúdo {string} do tipo {string}")]
    public async Task AdicionarConteudo(string titulo, string tipo) => await contexto.PostAsync($"/api/v1/palestras/{contexto.Ids["palestra"]}/conteudos", new
    {
        conteudoTitulo = titulo, conteudoTipo = tipo, conteudoUrl = "https://conteudo.teste.local/slides.pdf",
    });

    [When("tento adicionar novamente o palestrante")]
    public async Task AdicionarPalestranteDuplicado() => await contexto.PostAsync($"/api/v1/palestras/{contexto.Ids["palestra"]}/palestrantes", new
    {
        pessoaId = contexto.Ids["pessoa"], palestrantePapel = "Coautor",
    });

    [Then("a palestra deve possuir {int} palestrante\\(s\\)")]
    public async Task ValidarPalestrantes(int quantidade)
    {
        await contexto.GetAsync($"/api/v1/palestras/{contexto.Ids["palestra"]}");
        (await contexto.CorpoJsonAsync()).GetProperty("palestrantes").GetArrayLength().ShouldBe(quantidade);
    }

    [Then("a palestra deve possuir o conteúdo {string}")]
    public async Task ValidarConteudo(string titulo)
    {
        await contexto.GetAsync($"/api/v1/palestras/{contexto.Ids["palestra"]}");
        (await contexto.CorpoJsonAsync()).GetProperty("conteudos").EnumerateArray().Select(x => x.GetProperty("conteudoTitulo").GetString()).ShouldContain(titulo);
    }

    private object Payload(object palestrantes) => new
    {
        eventoId = contexto.Ids["evento"], trilhaId = contexto.Ids["trilha"], salaId = contexto.Ids["sala"], palestraTitulo = contexto.NomeUnico("Monolito Modular"),
        palestraInicio = Inicio.AddHours(1), palestraFim = Inicio.AddHours(2), palestrantes,
    };
}
