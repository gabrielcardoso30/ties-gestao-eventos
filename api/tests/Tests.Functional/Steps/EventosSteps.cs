using System.Net;
using Reqnroll;
using Shouldly;

namespace Tests.Functional.Steps;

[Binding]
public sealed class EventosSteps(ContextoDoCenario contexto)
{
    [Given("que existe um evento remoto em rascunho")]
    [When("crio um evento remoto válido")]
    public async Task CriarEventoRemoto()
    {
        var inicio = DateTimeOffset.UtcNow.AddDays(5);
        var response = await contexto.PostAsync("/api/v1/eventos", new
        {
            eventoNome = contexto.NomeUnico("Evento Remoto"), eventoDataInicio = inicio,
            eventoDataFim = inicio.AddHours(3), eventoFormato = "Remoto",
            eventoLinkRemoto = "https://evento.teste.local/sala",
        });
        if (response.StatusCode == HttpStatusCode.Created)
            contexto.Ids["evento"] = (await contexto.CorpoJsonAsync()).GetProperty("id").GetGuid();
    }

    [When("tento criar um evento presencial sem local")]
    public async Task CriarPresencialSemLocal()
    {
        var inicio = DateTimeOffset.UtcNow.AddDays(5);
        await contexto.PostAsync("/api/v1/eventos", new { eventoNome = contexto.NomeUnico("Presencial"), eventoDataInicio = inicio, eventoDataFim = inicio.AddHours(2), eventoFormato = "Presencial" });
    }

    [When("tento criar um evento com período invertido")]
    public async Task CriarPeriodoInvertido()
    {
        var inicio = DateTimeOffset.UtcNow.AddDays(5);
        await contexto.PostAsync("/api/v1/eventos", new { eventoNome = contexto.NomeUnico("Invertido"), eventoDataInicio = inicio, eventoDataFim = inicio.AddHours(-1), eventoFormato = "Remoto", eventoLinkRemoto = "https://evento.teste.local" });
    }

    [When("excluo o evento")]
    public async Task ExcluirEvento() => await contexto.DeleteAsync($"/api/v1/eventos/{contexto.Ids["evento"]}");

    [Then("o evento deve estar na situação {string}")]
    public async Task ValidarSituacao(string situacao) => (await contexto.CorpoJsonAsync()).GetProperty("eventoSituacao").GetString().ShouldBe(situacao);

    [Then("o evento não deve mais ser encontrado")]
    public async Task EventoNaoEncontrado()
    {
        await contexto.GetAsync($"/api/v1/eventos/{contexto.Ids["evento"]}");
        contexto.UltimaResposta!.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [When("tento criar um evento remoto sem link")]
    public async Task CriarRemotoSemLink()
    {
        var inicio = DateTimeOffset.UtcNow.AddDays(5);
        await contexto.PostAsync("/api/v1/eventos", new { eventoNome = contexto.NomeUnico("Sem link"), eventoDataInicio = inicio, eventoDataFim = inicio.AddHours(2), eventoFormato = "Remoto" });
    }

    [When("tento criar um evento remoto com capacidade zero")]
    public async Task CriarCapacidadeZero()
    {
        var inicio = DateTimeOffset.UtcNow.AddDays(5);
        await contexto.PostAsync("/api/v1/eventos", new { eventoNome = contexto.NomeUnico("Sem capacidade"), eventoDataInicio = inicio, eventoDataFim = inicio.AddHours(2), eventoFormato = "Remoto", eventoLinkRemoto = "https://evento.teste.local", eventoCapacidadeMaxima = 0 });
    }

    [When("tento publicar o evento")]
    public async Task PublicarEvento() => await contexto.PatchAsync($"/api/v1/eventos/{contexto.Ids["evento"]}/situacao", new { eventoSituacao = "Publicado" });

    [When("crio um evento remoto com as trilhas {string} e {string}")]
    public async Task CriarComTrilhas(string primeira, string segunda)
    {
        var inicio = DateTimeOffset.UtcNow.AddDays(5);
        var resposta = await contexto.PostAsync("/api/v1/eventos", new { eventoNome = contexto.NomeUnico("Evento com trilhas"), eventoDataInicio = inicio, eventoDataFim = inicio.AddHours(4), eventoFormato = "Remoto", eventoLinkRemoto = "https://evento.test", trilhas = new[] { new { trilhaNome = primeira, trilhaCor = "#112233" }, new { trilhaNome = segunda, trilhaCor = "#445566" } } });
        if (resposta.StatusCode == HttpStatusCode.Created) contexto.Ids["evento"] = (await contexto.CorpoJsonAsync()).GetProperty("id").GetGuid();
    }

    [Then("o evento deve apresentar {int} trilhas")]
    public async Task ValidarQuantidadeTrilhas(int quantidade)
    {
        await contexto.GetAsync($"/api/v1/eventos/{contexto.Ids["evento"]}");
        (await contexto.CorpoJsonAsync()).GetProperty("trilhas").GetArrayLength().ShouldBe(quantidade);
    }

    [When("adiciono a trilha {string} ao evento")]
    public async Task AdicionarTrilha(string nome) => await contexto.PostAsync($"/api/v1/eventos/{contexto.Ids["evento"]}/trilhas", new { trilhaNome = nome, trilhaCor = "#123456" });

    [Then("a trilha criada deve se chamar {string}")]
    public async Task ValidarNomeTrilha(string nome) => (await contexto.CorpoJsonAsync()).GetProperty("trilhaNome").GetString().ShouldBe(nome);

    [When("tento excluir a única trilha do evento")]
    public async Task ExcluirUnicaTrilha()
    {
        await contexto.GetAsync($"/api/v1/eventos/{contexto.Ids["evento"]}");
        var trilhaId = (await contexto.CorpoJsonAsync()).GetProperty("trilhas")[0].GetProperty("id").GetGuid();
        await contexto.DeleteAsync($"/api/v1/eventos/{contexto.Ids["evento"]}/trilhas/{trilhaId}");
    }
}
