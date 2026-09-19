using FluentValidation.TestHelper;
using Module.Pessoas.UseCases.ListarPessoas;
using Xunit;

namespace Tests.Unit.Pessoas;

public sealed class ListarPessoasValidatorTests
{
    private readonly ListarPessoasValidator _validator = new();

    [Fact]
    public void Deve_aceitar_padrao() => _validator.TestValidate(new ListarPessoasRequest(null, null)).ShouldNotHaveAnyValidationErrors();

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Deve_exigir_pagina_maior_ou_igual_a_um(int pagina) =>
        _validator.TestValidate(new ListarPessoasRequest(null, null, pagina)).ShouldHaveValidationErrorFor(r => r.Pagina);

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public void Deve_limitar_tamanho_da_pagina(int tamanho) =>
        _validator.TestValidate(new ListarPessoasRequest(null, null, 1, tamanho)).ShouldHaveValidationErrorFor(r => r.TamanhoPagina);

    [Fact]
    public void Deve_limitar_busca_em_100_caracteres() =>
        _validator.TestValidate(new ListarPessoasRequest(new string('x', 101), null)).ShouldHaveValidationErrorFor(r => r.Busca);
}
