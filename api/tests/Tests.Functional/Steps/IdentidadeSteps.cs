using Reqnroll;
using Shouldly;

namespace Tests.Functional.Steps;

[Binding]
public sealed class IdentidadeSteps(ContextoDoCenario contexto)
{
    private string Email => $"usuario-{contexto.Sufixo}@teste.local";
    [When("autentico com o administrador inicial")]
    public async Task AutenticarAdministrador() => await contexto.PostAnonimoAsync("/api/v1/identidade/sessoes", new
    {
        usuarioEmail = "admin@gestaoeventos.local", senha = "Admin@123456",
    });

    [When("tento autenticar com senha inválida")]
    public async Task AutenticarSenhaInvalida() => await contexto.PostAnonimoAsync("/api/v1/identidade/sessoes", new
    {
        usuarioEmail = "admin@gestaoeventos.local", senha = "senha-incorreta",
    });

    [When("cadastro um usuário com perfil {string}")]
    public async Task CadastrarUsuario(string perfil) => await contexto.PostAsync("/api/v1/identidade/usuarios", new
    {
        usuarioNome = contexto.NomeUnico("Usuário Funcional"),
        usuarioEmail = Email,
        senha = "Senha@123456",
        perfis = new[] { perfil },
    });

    [Then("a sessão deve conter um token e o perfil {string}")]
    public async Task ValidarSessao(string perfil)
    {
        var json = await contexto.CorpoJsonAsync();
        json.GetProperty("accessToken").GetString().ShouldNotBeNullOrWhiteSpace();
        json.GetProperty("usuario").GetProperty("perfis").EnumerateArray().Select(x => x.GetString()).ShouldContain(perfil);
    }

    [Then("o usuário deve possuir o perfil {string}")]
    public async Task ValidarPerfil(string perfil) =>
        (await contexto.CorpoJsonAsync()).GetProperty("perfis").EnumerateArray().Select(x => x.GetString()).ShouldContain(perfil);

    [Given("que cadastrei um usuário com perfil {string}")]
    public async Task UsuarioJaCadastrado(string perfil) => await CadastrarUsuario(perfil);

    [When("tento cadastrar outro usuário com o mesmo e-mail")]
    public async Task CadastrarDuplicado() => await contexto.PostAsync("/api/v1/identidade/usuarios", new
    {
        usuarioNome = contexto.NomeUnico("Duplicado"), usuarioEmail = Email, senha = "Senha@123456", perfis = new[] { "Participante" },
    });

    [When("tento cadastrar um usuário com senha fraca")]
    public async Task CadastrarSenhaFraca() => await contexto.PostAsync("/api/v1/identidade/usuarios", new
    {
        usuarioNome = contexto.NomeUnico("Senha fraca"), usuarioEmail = Email, senha = "123", perfis = new[] { "Participante" },
    });
}
