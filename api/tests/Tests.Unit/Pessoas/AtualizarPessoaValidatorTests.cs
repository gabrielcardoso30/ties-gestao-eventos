using FluentValidation.TestHelper;
using Module.Pessoas.UseCases.AtualizarPessoa;
using Xunit;

namespace Tests.Unit.Pessoas;

public sealed class AtualizarPessoaValidatorTests
{
    private readonly AtualizarPessoaValidator _validator = new();

    private static AtualizarPessoaRequest RequestValido() =>
        new("Maria Silva", "maria@empresa.com", null, "529.982.247-25", "Globalsys", null, null, null, EstaAtivo: false) { PessoaId = Guid.NewGuid() };

    [Fact]
    public void Deve_aceitar_request_valido() => _validator.TestValidate(RequestValido()).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Deve_exigir_nome() =>
        _validator.TestValidate(RequestValido() with { PessoaNome = "" }).ShouldHaveValidationErrorFor(r => r.PessoaNome);

    [Fact]
    public void Deve_exigir_email_valido() =>
        _validator.TestValidate(RequestValido() with { PessoaEmail = "invalido" }).ShouldHaveValidationErrorFor(r => r.PessoaEmail);

    [Fact]
    public void Deve_rejeitar_cpf_invalido() =>
        _validator.TestValidate(RequestValido() with { PessoaDocumento = "123.456.789-00" }).ShouldHaveValidationErrorFor(r => r.PessoaDocumento);

    [Fact]
    public void Deve_limitar_tamanho_do_cargo() =>
        _validator.TestValidate(RequestValido() with { PessoaCargo = new string('c', 101) }).ShouldHaveValidationErrorFor(r => r.PessoaCargo);
}
