using FluentValidation.TestHelper;
using Module.Pessoas.UseCases.CriarPessoa;
using Xunit;

namespace Tests.Unit.Pessoas;

public sealed class CriarPessoaValidatorTests
{
    private readonly CriarPessoaValidator _validator = new();

    private static CriarPessoaRequest RequestValido() =>
        new("Maria Silva", "maria@empresa.com", "(27) 99999-0000", "529.982.247-25", "Globalsys", "Arquiteta", "Bio", "https://foto.local/m.png");

    [Fact]
    public void Deve_aceitar_request_valido() => _validator.TestValidate(RequestValido()).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Deve_aceitar_opcionais_nulos() =>
        _validator.TestValidate(new CriarPessoaRequest("Maria", "maria@empresa.com", null, null, null, null, null, null)).ShouldNotHaveAnyValidationErrors();

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Deve_exigir_nome(string nome) =>
        _validator.TestValidate(RequestValido() with { PessoaNome = nome }).ShouldHaveValidationErrorFor(r => r.PessoaNome);

    [Fact]
    public void Deve_limitar_tamanho_do_nome() =>
        _validator.TestValidate(RequestValido() with { PessoaNome = new string('a', 151) }).ShouldHaveValidationErrorFor(r => r.PessoaNome);

    [Theory]
    [InlineData("")]
    [InlineData("nao-e-email")]
    [InlineData("a@")]
    public void Deve_exigir_email_valido(string email) =>
        _validator.TestValidate(RequestValido() with { PessoaEmail = email }).ShouldHaveValidationErrorFor(r => r.PessoaEmail);

    [Theory]
    [InlineData("123")]
    [InlineData("111.111.111-11")]
    [InlineData("529.982.247-26")]
    public void Deve_rejeitar_cpf_invalido(string cpf) =>
        _validator.TestValidate(RequestValido() with { PessoaDocumento = cpf }).ShouldHaveValidationErrorFor(r => r.PessoaDocumento);

    [Theory]
    [InlineData("52998224725")]
    [InlineData("529.982.247-25")]
    public void Deve_aceitar_cpf_valido_com_ou_sem_mascara(string cpf) =>
        _validator.TestValidate(RequestValido() with { PessoaDocumento = cpf }).ShouldNotHaveValidationErrorFor(r => r.PessoaDocumento);

    [Fact]
    public void Deve_limitar_minibio_em_2000_caracteres() =>
        _validator.TestValidate(RequestValido() with { PessoaMiniBio = new string('b', 2001) }).ShouldHaveValidationErrorFor(r => r.PessoaMiniBio);

    [Theory]
    [InlineData("foto.png")]
    [InlineData("ftp://servidor/foto.png")]
    [InlineData("javascript:alert(1)")]
    public void Deve_rejeitar_url_de_foto_nao_http(string url) =>
        _validator.TestValidate(RequestValido() with { PessoaFotoUrl = url }).ShouldHaveValidationErrorFor(r => r.PessoaFotoUrl);
}
