using System.Net;
using System.Net.Http.Json;
using Shouldly;
using Tests.Integration.Infra;
using Xunit;

namespace Tests.Integration.Eventos;

[Collection(ApiCollection.Nome)]
public sealed class EventosTrilhasEndpointsTests(ApiFactory factory)
{
    private async Task<EventoDetalhe> CriarAsync(HttpClient client)
    {
        var inicio = DateTimeOffset.UtcNow.AddDays(10);
        var response = await client.PostAsJsonAsync("/api/v1/eventos", new { eventoNome = $"Evento {Guid.NewGuid():N}", eventoDataInicio = inicio, eventoDataFim = inicio.AddHours(8), eventoFormato = "Remoto", eventoLinkRemoto = "https://evento.test", trilhas = new[] { new { trilhaNome = "Arquitetura", trilhaCor = "#112233" } } });
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        return (await client.GetFromJsonAsync<EventoDetalhe>(response.Headers.Location))!;
    }

    [Fact]
    public async Task Criar_evento_com_trilha_e_manter_crud_com_soft_delete()
    {
        var client = factory.ClienteAutenticado(); var evento = await CriarAsync(client);
        evento.Trilhas.ShouldHaveSingleItem().TrilhaNome.ShouldBe("Arquitetura");
        var segunda = await client.PostAsJsonAsync($"/api/v1/eventos/{evento.Id}/trilhas", new { trilhaNome = "Frontend", trilhaDescricao = "Interfaces", trilhaCor = "#AABBCC" });
        segunda.StatusCode.ShouldBe(HttpStatusCode.Created); var criada = (await segunda.Content.ReadFromJsonAsync<TrilhaDetalhe>())!;
        (await client.PutAsJsonAsync($"/api/v1/eventos/{evento.Id}/trilhas/{criada.Id}", new { trilhaNome = "Web", trilhaDescricao = "React", trilhaCor = "#CCBBAA", estaAtivo = false })).StatusCode.ShouldBe(HttpStatusCode.OK);
        (await client.DeleteAsync($"/api/v1/eventos/{evento.Id}/trilhas/{criada.Id}")).StatusCode.ShouldBe(HttpStatusCode.NoContent);
        var ultima = evento.Trilhas.Single(); var bloqueio = await client.DeleteAsync($"/api/v1/eventos/{evento.Id}/trilhas/{ultima.Id}");
        bloqueio.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        (await ProblemDetailsDeTeste.LerAsync(bloqueio)).Codigo.ShouldBe("Eventos.EventoPrecisaDeUmaTrilha");
    }

    [Fact]
    public async Task Nome_duplicado_e_cor_invalida_devem_retornar_problem_details()
    {
        var client = factory.ClienteAutenticado(); var evento = await CriarAsync(client);
        var duplicada = await client.PostAsJsonAsync($"/api/v1/eventos/{evento.Id}/trilhas", new { trilhaNome = "arquitetura", trilhaCor = "#FFFFFF" });
        duplicada.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        var invalida = await client.PostAsJsonAsync($"/api/v1/eventos/{evento.Id}/trilhas", new { trilhaNome = "Dados", trilhaCor = "azul" });
        invalida.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    private sealed record EventoDetalhe(Guid Id, List<TrilhaDetalhe> Trilhas);
    private sealed record TrilhaDetalhe(Guid Id, string TrilhaNome, bool EstaAtivo);
}
