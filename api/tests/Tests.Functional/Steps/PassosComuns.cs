using System.Net;
using Reqnroll;
using Shouldly;
using Tests.Integration.Infra;

namespace Tests.Functional.Steps;

[Binding]
public sealed class PassosComuns(ContextoDoCenario contexto)
{
    [Given("que estou autenticado como {string}")]
    public void DadoQueEstouAutenticadoComo(string perfil) => contexto.AutenticarComo(perfil);

    [Then("a resposta deve ter status {int}")]
    public async Task EntaoARespostaDeveTerStatus(int status)
    {
        var corpo = await contexto.UltimaResposta!.Content.ReadAsStringAsync();
        ((int)contexto.UltimaResposta.StatusCode).ShouldBe(status, corpo);
    }

    [Then("a resposta deve ser um problema com código {string}")]
    public async Task EntaoARespostaDeveSerUmProblemaComCodigo(string codigo)
    {
        contexto.UltimaResposta!.Content.Headers.ContentType!.MediaType.ShouldBe("application/problem+json");
        var problema = await ProblemDetailsDeTeste.LerAsync(contexto.UltimaResposta);
        problema.Codigo.ShouldBe(codigo);
        problema.TraceId.ShouldNotBeNullOrWhiteSpace();
    }

    [Then("a resposta deve ser não autorizada")]
    public void EntaoARespostaDeveSerNaoAutorizada() => contexto.UltimaResposta!.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
}
