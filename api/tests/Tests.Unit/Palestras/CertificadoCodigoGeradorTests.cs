using Module.Palestras.Domain;
using Shouldly;

namespace Tests.Unit.Palestras;

public class CertificadoCodigoGeradorTests
{
    [Fact]
    public void Gerar_deve_produzir_12_caracteres_do_alfabeto_sem_ambiguos()
    {
        for (var i = 0; i < 500; i++)
        {
            var codigo = CertificadoCodigoGerador.Gerar();

            codigo.Length.ShouldBe(12);
            codigo.ShouldAllBe(c => CertificadoCodigoGerador.Alfabeto.Contains(c));
            codigo.ShouldNotContain('0');
            codigo.ShouldNotContain('O');
            codigo.ShouldNotContain('1');
            codigo.ShouldNotContain('I');
            codigo.ShouldNotContain('L');
        }
    }

    [Fact]
    public void Gerar_deve_produzir_codigos_distintos()
    {
        var codigos = Enumerable.Range(0, 1000).Select(_ => CertificadoCodigoGerador.Gerar()).ToHashSet(StringComparer.Ordinal);

        codigos.Count.ShouldBe(1000);
    }

    [Fact]
    public void Alfabeto_deve_conter_apenas_os_31_simbolos_nao_ambiguos()
    {
        CertificadoCodigoGerador.Alfabeto.Length.ShouldBe(31);
        CertificadoCodigoGerador.Alfabeto.Distinct().Count().ShouldBe(31);
    }
}
