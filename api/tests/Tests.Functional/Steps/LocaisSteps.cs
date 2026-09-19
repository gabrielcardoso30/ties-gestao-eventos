using System.Net;
using Reqnroll;
using Shouldly;

namespace Tests.Functional.Steps;

[Binding]
public sealed class LocaisSteps(ContextoDoCenario contexto)
{
    [Given("que existe um local chamado {string} com ambiente único para {int} pessoas")]
    [When("crio um local chamado {string} com ambiente único para {int} pessoas")]
    public async Task QuandoCrioUmLocalComAmbienteUnico(string nome, int capacidade)
    {
        var resposta = await contexto.PostAsync("/api/v1/locais", new
        {
            localNome = contexto.NomeUnico(nome),
            enderecoCidade = "Vila Velha",
            enderecoUf = "ES",
            capacidadeAmbienteUnico = capacidade,
        });
        if (resposta.StatusCode == HttpStatusCode.Created)
        {
            var corpo = await contexto.CorpoJsonAsync();
            contexto.Ids["local"] = corpo.GetProperty("id").GetGuid();
        }
    }

    [When("adiciono a sala {string} com capacidade {int} do tipo {string}")]
    public async Task QuandoAdicionoASala(string nome, int capacidade, string tipo)
    {
        var resposta = await contexto.PostAsync($"/api/v1/locais/{contexto.Ids["local"]}/salas", new { salaNome = nome, salaCapacidade = capacidade, salaTipo = tipo });
        if (resposta.StatusCode == HttpStatusCode.Created)
        {
            contexto.Ids[$"sala:{nome}"] = (await contexto.CorpoJsonAsync()).GetProperty("id").GetGuid();
        }
    }

    [When("tento excluir a sala {string}")]
    public async Task QuandoTentoExcluirASala(string nome)
    {
        if (!contexto.Ids.TryGetValue($"sala:{nome}", out var salaId))
        {
            await contexto.GetAsync($"/api/v1/locais/{contexto.Ids["local"]}");
            var salas = (await contexto.CorpoJsonAsync()).GetProperty("salas").EnumerateArray();
            salaId = salas.First(s => s.GetProperty("salaNome").GetString() == nome).GetProperty("id").GetGuid();
        }

        await contexto.DeleteAsync($"/api/v1/locais/{contexto.Ids["local"]}/salas/{salaId}");
    }

    [Then("o local deve possuir {int} sala\\(s\\)")]
    public async Task EntaoOLocalDevePossuirSalas(int quantidade)
    {
        await contexto.GetAsync($"/api/v1/locais/{contexto.Ids["local"]}");
        var corpo = await contexto.CorpoJsonAsync();
        corpo.GetProperty("salas").GetArrayLength().ShouldBe(quantidade);
    }

    [Then("a capacidade total do local deve ser {int}")]
    public async Task EntaoACapacidadeTotalDeveSer(int capacidade)
    {
        await contexto.GetAsync($"/api/v1/locais/{contexto.Ids["local"]}");
        (await contexto.CorpoJsonAsync()).GetProperty("localCapacidadeTotal").GetInt32().ShouldBe(capacidade);
    }

    [Then("a primeira sala deve se chamar {string} e ser do tipo {string}")]
    public async Task EntaoAPrimeiraSalaDeveSeChamar(string nome, string tipo)
    {
        await contexto.GetAsync($"/api/v1/locais/{contexto.Ids["local"]}");
        var sala = (await contexto.CorpoJsonAsync()).GetProperty("salas")[0];
        sala.GetProperty("salaNome").GetString().ShouldBe(nome);
        sala.GetProperty("salaTipo").GetString().ShouldBe(tipo);
    }
}
