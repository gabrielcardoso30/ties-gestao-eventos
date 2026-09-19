using Xunit;
using FluentValidation.TestHelper;
using Module.Identidade.UseCases.AtualizarPerfisUsuario;
using Module.Identidade.UseCases.CriarSessao;
using Module.Identidade.UseCases.ListarUsuarios;
using Module.Identidade.UseCases.ObterUsuarioAtual;
using Module.Identidade.UseCases.RegistrarUsuario;
using Shared.Contracts.Identidade;
using Shouldly;

namespace Tests.Unit.Identidade;

public sealed class CriarSessaoValidatorTests
{
    private readonly CriarSessaoValidator _validator = new();

    [Fact]
    public void RequestValidoDevePassar() =>
        _validator.TestValidate(new CriarSessaoRequest("admin@gestaoeventos.local", "Admin@123456")).ShouldNotHaveAnyValidationErrors();

    [Theory]
    [InlineData("")]
    [InlineData("nao-e-email")]
    public void EmailInvalidoDeveFalhar(string email) =>
        _validator.TestValidate(new CriarSessaoRequest(email, "Admin@123456")).ShouldHaveValidationErrorFor(r => r.UsuarioEmail);

    [Fact]
    public void SenhaVaziaDeveFalhar() =>
        _validator.TestValidate(new CriarSessaoRequest("admin@gestaoeventos.local", "")).ShouldHaveValidationErrorFor(r => r.Senha);
}

public sealed class RegistrarUsuarioValidatorTests
{
    private readonly RegistrarUsuarioValidator _validator = new();

    [Fact]
    public void RequestValidoDevePassar() =>
        _validator.TestValidate(new RegistrarUsuarioRequest("Maria", "maria@exemplo.com", "Senha@123", [PerfisPadrao.Organizador])).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void NomeVazioDeveFalhar() =>
        _validator.TestValidate(new RegistrarUsuarioRequest("", "maria@exemplo.com", "Senha@123", [PerfisPadrao.Organizador])).ShouldHaveValidationErrorFor(r => r.UsuarioNome);

    [Fact]
    public void NomeAcimaDe150DeveFalhar() =>
        _validator.TestValidate(new RegistrarUsuarioRequest(new string('a', 151), "maria@exemplo.com", "Senha@123", [PerfisPadrao.Organizador])).ShouldHaveValidationErrorFor(r => r.UsuarioNome);

    [Fact]
    public void EmailInvalidoDeveFalhar() =>
        _validator.TestValidate(new RegistrarUsuarioRequest("Maria", "maria", "Senha@123", [PerfisPadrao.Organizador])).ShouldHaveValidationErrorFor(r => r.UsuarioEmail);

    [Fact]
    public void SenhaVaziaDeveFalhar() =>
        _validator.TestValidate(new RegistrarUsuarioRequest("Maria", "maria@exemplo.com", "", [PerfisPadrao.Organizador])).ShouldHaveValidationErrorFor(r => r.Senha);

    [Fact]
    public void ListaDePerfisVaziaDeveFalhar() =>
        _validator.TestValidate(new RegistrarUsuarioRequest("Maria", "maria@exemplo.com", "Senha@123", [])).ShouldHaveValidationErrorFor(r => r.Perfis);

    [Fact]
    public void PerfilVazioNaListaDeveFalhar()
    {
        var resultado = _validator.TestValidate(new RegistrarUsuarioRequest("Maria", "maria@exemplo.com", "Senha@123", [""]));
        resultado.IsValid.ShouldBeFalse();
        resultado.Errors.ShouldContain(e => e.PropertyName == "Perfis[0]");
    }
}

public sealed class AtualizarPerfisUsuarioValidatorTests
{
    private readonly AtualizarPerfisUsuarioValidator _validator = new();

    [Fact]
    public void RequestValidoDevePassar() =>
        _validator.TestValidate(new AtualizarPerfisUsuarioRequest([PerfisPadrao.Participante]) { UsuarioId = Guid.CreateVersion7() }).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void ListaVaziaEhPermitida() =>
        _validator.TestValidate(new AtualizarPerfisUsuarioRequest([]) { UsuarioId = Guid.CreateVersion7() }).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void UsuarioIdVazioDeveFalhar() =>
        _validator.TestValidate(new AtualizarPerfisUsuarioRequest([PerfisPadrao.Participante])).ShouldHaveValidationErrorFor(r => r.UsuarioId);

    [Fact]
    public void PerfisNuloDeveFalhar() =>
        _validator.TestValidate(new AtualizarPerfisUsuarioRequest(null!) { UsuarioId = Guid.CreateVersion7() }).ShouldHaveValidationErrorFor(r => r.Perfis);
}

public sealed class ListarUsuariosValidatorTests
{
    private readonly ListarUsuariosValidator _validator = new();

    [Fact]
    public void PadraoDevePassar() =>
        _validator.TestValidate(new ListarUsuariosRequest(null, null)).ShouldNotHaveAnyValidationErrors();

    [Theory]
    [InlineData(0, 20)]
    [InlineData(1, 0)]
    [InlineData(1, 101)]
    public void PaginacaoForaDosLimitesDeveFalhar(int pagina, int tamanho) =>
        _validator.TestValidate(new ListarUsuariosRequest(null, null, pagina, tamanho)).IsValid.ShouldBeFalse();

    [Fact]
    public void BuscaAcimaDe100DeveFalhar() =>
        _validator.TestValidate(new ListarUsuariosRequest(new string('x', 101), null)).ShouldHaveValidationErrorFor(r => r.Busca);
}

public sealed class ObterUsuarioAtualValidatorTests
{
    [Fact]
    public void GuidVazioDeveFalhar() =>
        new ObterUsuarioAtualValidator().TestValidate(new ObterUsuarioAtualRequest(Guid.Empty)).ShouldHaveValidationErrorFor(r => r.UsuarioId);
}
