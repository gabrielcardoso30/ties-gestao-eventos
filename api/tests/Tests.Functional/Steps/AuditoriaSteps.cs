using Reqnroll;
using Shouldly;

namespace Tests.Functional.Steps;

[Binding]
public sealed class AuditoriaSteps(ContextoDoCenario contexto)
{
    [When("consulto os registros de auditoria")]
    public async Task Consultar() => await contexto.GetAsync("/api/v1/auditoria/registros?pagina=1&tamanhoPagina=10");

    [Then("a auditoria deve retornar uma coleção paginada")]
    public async Task ValidarPaginacao()
    {
        var json = await contexto.CorpoJsonAsync();
        json.TryGetProperty("itens", out var itens).ShouldBeTrue();
        itens.ValueKind.ShouldBe(System.Text.Json.JsonValueKind.Array);
        json.GetProperty("pagina").GetInt32().ShouldBe(1);
    }
}
