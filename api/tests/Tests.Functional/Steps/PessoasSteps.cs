using System.Net;
using Reqnroll;
using Shouldly;

namespace Tests.Functional.Steps;

[Binding]
public sealed class PessoasSteps(ContextoDoCenario contexto)
{
    private string Email => $"pessoa-{contexto.Sufixo}@teste.local";

    [Given("que cadastrei uma pessoa chamada {string}")]
    [When("cadastro uma pessoa chamada {string}")]
    public async Task CadastrarPessoa(string nome)
    {
        var resposta = await contexto.PostAsync("/api/v1/pessoas", new
        {
            pessoaNome = contexto.NomeUnico(nome),
            pessoaEmail = Email,
            pessoaEmpresa = "TIES",
        });
        if (resposta.StatusCode == HttpStatusCode.Created)
            contexto.Ids["pessoa"] = (await contexto.CorpoJsonAsync()).GetProperty("id").GetGuid();
    }

    [When("tento cadastrar outra pessoa com o mesmo e-mail")]
    public async Task CadastrarPessoaDuplicada() => await contexto.PostAsync("/api/v1/pessoas", new
    {
        pessoaNome = contexto.NomeUnico("Pessoa duplicada"),
        pessoaEmail = Email,
    });

    [When("excluo a pessoa cadastrada")]
    public async Task ExcluirPessoa() => await contexto.DeleteAsync($"/api/v1/pessoas/{contexto.Ids["pessoa"]}");

    [Then("consigo consultar a pessoa cadastrada")]
    public async Task ConsultarPessoa()
    {
        await contexto.GetAsync($"/api/v1/pessoas/{contexto.Ids["pessoa"]}");
        contexto.UltimaResposta!.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await contexto.CorpoJsonAsync()).GetProperty("pessoaEmail").GetString().ShouldBe(Email);
    }

    [Then("a pessoa cadastrada não deve mais ser encontrada")]
    public async Task PessoaNaoEncontrada()
    {
        await contexto.GetAsync($"/api/v1/pessoas/{contexto.Ids["pessoa"]}");
        contexto.UltimaResposta!.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [When("tento cadastrar uma pessoa com e-mail inválido")]
    public async Task CadastrarEmailInvalido() => await contexto.PostAsync("/api/v1/pessoas", new
    {
        pessoaNome = contexto.NomeUnico("E-mail inválido"), pessoaEmail = "email-invalido",
    });

    [When("altero o nome da pessoa para {string}")]
    public async Task AlterarNome(string nome) => await contexto.PutAsync($"/api/v1/pessoas/{contexto.Ids["pessoa"]}", new
    {
        pessoaNome = nome, pessoaEmail = Email, estaAtivo = true,
    });

    [Then("consigo consultar a pessoa com o nome {string}")]
    public async Task ConsultarNome(string nome)
    {
        await contexto.GetAsync($"/api/v1/pessoas/{contexto.Ids["pessoa"]}");
        (await contexto.CorpoJsonAsync()).GetProperty("pessoaNome").GetString().ShouldBe(nome);
    }
}
