using System.Net.Http.Json;
using System.Text.Json;
using Shared.Contracts.Identidade;
using Tests.Functional.Hooks;
using Tests.Integration.Infra;

namespace Tests.Functional.Steps;

/// <summary>Estado compartilhado entre os passos de um cenário (injeção de contexto do Reqnroll).</summary>
public sealed class ContextoDoCenario
{
    private HttpClient? _client;

    public HttpClient Client => _client ??= ApiHooks.Api.ClienteAutenticado(PerfisPadrao.Administrador, "Organizador Funcional");
    public HttpResponseMessage? UltimaResposta { get; set; }
    public Dictionary<string, Guid> Ids { get; } = [];
    public string Sufixo { get; } = Guid.NewGuid().ToString("N")[..8];

    public void AutenticarComo(string perfil) => _client = ApiHooks.Api.ClienteAutenticado(perfil, $"Usuário {perfil}");

    public string NomeUnico(string nome) => $"{nome} [{Sufixo}]";

    public async Task<JsonElement> CorpoJsonAsync()
    {
        var texto = await UltimaResposta!.Content.ReadAsStringAsync();
        return JsonDocument.Parse(texto).RootElement.Clone();
    }

    public async Task<HttpResponseMessage> PostAsync(string url, object corpo)
    {
        UltimaResposta = await Client.PostAsJsonAsync(url, corpo);
        return UltimaResposta;
    }

    public async Task<HttpResponseMessage> PutAsync(string url, object corpo)
    {
        UltimaResposta = await Client.PutAsJsonAsync(url, corpo);
        return UltimaResposta;
    }

    public async Task<HttpResponseMessage> PatchAsync(string url, object corpo)
    {
        UltimaResposta = await Client.PatchAsJsonAsync(url, corpo);
        return UltimaResposta;
    }

    public async Task<HttpResponseMessage> GetAsync(string url)
    {
        UltimaResposta = await Client.GetAsync(url);
        return UltimaResposta;
    }

    public async Task<HttpResponseMessage> DeleteAsync(string url)
    {
        UltimaResposta = await Client.DeleteAsync(url);
        return UltimaResposta;
    }
}
