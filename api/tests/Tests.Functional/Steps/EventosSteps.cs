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
}
