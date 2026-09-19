using Module.Pessoas.Domain;
using Shouldly;
using Xunit;

namespace Tests.Unit.Pessoas;

public sealed class CpfTests
{
    [Theory]
    [InlineData("529.982.247-25")]
    [InlineData("52998224725")]
    [InlineData(" 529 982 247 25 ")]
    [InlineData("111.444.777-35")]
    public void EhValido_deve_aceitar_cpf_com_digitos_verificadores_corretos(string cpf) => Cpf.EhValido(cpf).ShouldBeTrue();

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("abc")]
    [InlineData("1234567890")]
    [InlineData("123456789012")]
    [InlineData("529.982.247-26")]
    [InlineData("111.111.111-11")]
    [InlineData("000.000.000-00")]
    public void EhValido_deve_rejeitar_cpf_invalido(string? cpf) => Cpf.EhValido(cpf).ShouldBeFalse();

    [Theory]
    [InlineData("529.982.247-25", "52998224725")]
    [InlineData("52998224725", "52998224725")]
    [InlineData(" 529 982 247 25 ", "52998224725")]
    public void Normalizar_deve_manter_apenas_digitos(string entrada, string esperado) => Cpf.Normalizar(entrada).ShouldBe(esperado);

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("---")]
    public void Normalizar_deve_retornar_nulo_sem_digitos(string? entrada) => Cpf.Normalizar(entrada).ShouldBeNull();
}
