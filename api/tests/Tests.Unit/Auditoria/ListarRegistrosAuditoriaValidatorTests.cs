using FluentValidation.TestHelper;
using Module.Auditoria.UseCases.ListarRegistrosAuditoria;
using Xunit;

namespace Tests.Unit.Auditoria;

public sealed class ListarRegistrosAuditoriaValidatorTests
{
    private readonly ListarRegistrosAuditoriaValidator _validator = new();

    private static ListarRegistrosAuditoriaRequest Vazio() => new(null, null, null, null, null, null, null);

    [Fact]
    public void Deve_aceitar_request_sem_filtros() => _validator.TestValidate(Vazio()).ShouldNotHaveAnyValidationErrors();

    [Theory]
    [InlineData("Inclusao")]
    [InlineData("alteracao")]
    [InlineData("EXCLUSAO")]
    public void Deve_aceitar_operacoes_conhecidas_sem_distinguir_caixa(string operacao) =>
        _validator.TestValidate(Vazio() with { Operacao = operacao }).ShouldNotHaveValidationErrorFor(r => r.Operacao);

    [Fact]
    public void Deve_rejeitar_operacao_desconhecida() =>
        _validator.TestValidate(Vazio() with { Operacao = "Leitura" }).ShouldHaveValidationErrorFor(r => r.Operacao);

    [Fact]
    public void Deve_rejeitar_intervalo_invertido()
    {
        var de = new DateTimeOffset(2026, 9, 18, 0, 0, 0, TimeSpan.Zero);
        _validator.TestValidate(Vazio() with { OcorridoDe = de, OcorridoAte = de.AddDays(-1) }).ShouldHaveValidationErrorFor(r => r.OcorridoAte);
    }

    [Fact]
    public void Deve_aceitar_intervalo_com_limites_iguais()
    {
        var de = new DateTimeOffset(2026, 9, 18, 0, 0, 0, TimeSpan.Zero);
        _validator.TestValidate(Vazio() with { OcorridoDe = de, OcorridoAte = de }).ShouldNotHaveValidationErrorFor(r => r.OcorridoAte);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public void Deve_limitar_tamanho_da_pagina(int tamanho) =>
        _validator.TestValidate(Vazio() with { TamanhoPagina = tamanho }).ShouldHaveValidationErrorFor(r => r.TamanhoPagina);

    [Fact]
    public void Deve_limitar_tamanho_do_modulo() =>
        _validator.TestValidate(Vazio() with { Modulo = new string('m', 51) }).ShouldHaveValidationErrorFor(r => r.Modulo);
}
