using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Shouldly;
using Tests.Integration.Infra;
using Xunit;

namespace Tests.Integration.Modulos;

[Collection(ApiCollection.Nome)]
public sealed class ModulosEndpointsTests(ApiFactory factory)
{
    [Fact]
    public async Task Identidade_deve_registrar_usuario_e_autenticar_com_identity()
    {
        var admin = factory.ClienteAutenticado();
        var sufixo = Guid.NewGuid().ToString("N");
        var email = $"usuario-{sufixo}@teste.local";
        var senha = "Senha@123456";

        var registro = await admin.PostAsJsonAsync("/api/v1/identidade/usuarios", new
        {
            usuarioNome = $"Usuário {sufixo}", usuarioEmail = email, senha, perfis = new[] { "Organizador" },
        });
        registro.StatusCode.ShouldBe(HttpStatusCode.Created);

        var login = await factory.CreateClient().PostAsJsonAsync("/api/v1/identidade/sessoes", new { usuarioEmail = email, senha });
        login.StatusCode.ShouldBe(HttpStatusCode.OK);
        var json = await login.Content.ReadFromJsonAsync<JsonElement>();
        json.GetProperty("accessToken").GetString().ShouldNotBeNullOrWhiteSpace();
        json.GetProperty("usuario").GetProperty("perfis")[0].GetString().ShouldBe("Organizador");
    }

    [Fact]
    public async Task Evento_remoto_deve_ser_criado_e_retornado_sem_local()
    {
        var client = factory.ClienteAutenticado();
        var inicio = DateTimeOffset.UtcNow.AddDays(2);
        var response = await client.PostAsJsonAsync("/api/v1/eventos", new
        {
            eventoNome = $"Evento remoto {Guid.NewGuid():N}",
            eventoDataInicio = inicio,
            eventoDataFim = inicio.AddHours(4),
            eventoFormato = "Remoto",
            eventoLinkRemoto = "https://evento.teste.local/sala",
            eventoCapacidadeMaxima = 500,
        });
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var criado = await response.Content.ReadFromJsonAsync<JsonElement>();

        var detalhe = await client.GetFromJsonAsync<JsonElement>($"/api/v1/eventos/{criado.GetProperty("id").GetGuid()}");
        detalhe.GetProperty("eventoFormato").GetString().ShouldBe("Remoto");
        if (detalhe.TryGetProperty("localId", out var localId)) localId.ValueKind.ShouldBe(JsonValueKind.Null);
        detalhe.GetProperty("eventoSituacao").GetString().ShouldBe("Rascunho");
    }

    [Fact]
    public async Task Evento_presencial_sem_local_deve_retornar_problem_details_de_negocio()
    {
        var client = factory.ClienteAutenticado();
        var inicio = DateTimeOffset.UtcNow.AddDays(2);
        var response = await client.PostAsJsonAsync("/api/v1/eventos", new
        {
            eventoNome = $"Evento inválido {Guid.NewGuid():N}",
            eventoDataInicio = inicio,
            eventoDataFim = inicio.AddHours(2),
            eventoFormato = "Presencial",
        });
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem = await ProblemDetailsDeTeste.LerAsync(response);
        problem.Codigo.ShouldBe("Validacao");
        problem.Errors!.Keys.ShouldContain("LocalId");
    }
}
