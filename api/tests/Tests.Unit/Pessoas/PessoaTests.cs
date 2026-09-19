using Module.Pessoas.Domain;
using Shared.Contracts.Pessoas;
using Shouldly;
using Xunit;

namespace Tests.Unit.Pessoas;

public sealed class PessoaTests
{
    private static Pessoa CriarPessoaPadrao() =>
        Pessoa.Criar("  Maria Silva ", "  Maria.Silva@Empresa.COM ", " (27) 99999-0000 ", "529.982.247-25", "  Globalsys ", "", "   ", null);

    [Fact]
    public void Criar_deve_normalizar_email_documento_e_campos_opcionais()
    {
        var pessoa = CriarPessoaPadrao();

        pessoa.Id.ShouldNotBe(Guid.Empty);
        pessoa.PessoaNome.ShouldBe("Maria Silva");
        pessoa.PessoaEmail.ShouldBe("maria.silva@empresa.com");
        pessoa.PessoaTelefone.ShouldBe("(27) 99999-0000");
        pessoa.PessoaDocumento.ShouldBe("52998224725");
        pessoa.PessoaEmpresa.ShouldBe("Globalsys");
        pessoa.PessoaCargo.ShouldBeNull();
        pessoa.PessoaMiniBio.ShouldBeNull();
        pessoa.PessoaFotoUrl.ShouldBeNull();
        pessoa.EstaAtivo.ShouldBeTrue();
    }

    [Fact]
    public void Criar_deve_registrar_evento_PessoaCriada_com_dados_normalizados()
    {
        var pessoa = CriarPessoaPadrao();

        var evento = pessoa.Eventos.ShouldHaveSingleItem().ShouldBeOfType<PessoaCriada>();
        evento.PessoaId.ShouldBe(pessoa.Id);
        evento.PessoaNome.ShouldBe("Maria Silva");
        evento.PessoaEmail.ShouldBe("maria.silva@empresa.com");
    }

    [Fact]
    public void Criar_sem_documento_deve_manter_documento_nulo()
    {
        var pessoa = Pessoa.Criar("João", "joao@teste.local", null, "  ", null, null, null, null);

        pessoa.PessoaDocumento.ShouldBeNull();
    }

    [Fact]
    public void Atualizar_deve_substituir_todos_os_campos_e_situacao()
    {
        var pessoa = CriarPessoaPadrao();
        pessoa.LimparEventos();

        pessoa.Atualizar("Maria S.", "OUTRO@EMAIL.COM", null, null, null, "Arquiteta", "Bio", "https://foto.local/m.png", estaAtivo: false);

        pessoa.PessoaNome.ShouldBe("Maria S.");
        pessoa.PessoaEmail.ShouldBe("outro@email.com");
        pessoa.PessoaTelefone.ShouldBeNull();
        pessoa.PessoaDocumento.ShouldBeNull();
        pessoa.PessoaEmpresa.ShouldBeNull();
        pessoa.PessoaCargo.ShouldBe("Arquiteta");
        pessoa.PessoaMiniBio.ShouldBe("Bio");
        pessoa.PessoaFotoUrl.ShouldBe("https://foto.local/m.png");
        pessoa.EstaAtivo.ShouldBeFalse();
        pessoa.Eventos.ShouldBeEmpty();
    }

    [Fact]
    public void MarcarExcluida_deve_registrar_evento_PessoaExcluida()
    {
        var pessoa = CriarPessoaPadrao();
        pessoa.LimparEventos();

        pessoa.MarcarExcluida();

        pessoa.Eventos.ShouldHaveSingleItem().ShouldBeOfType<PessoaExcluida>().PessoaId.ShouldBe(pessoa.Id);
    }

    [Theory]
    [InlineData("  A@B.com ", "a@b.com")]
    [InlineData("x@y.z", "x@y.z")]
    public void NormalizarEmail_deve_aplicar_trim_e_minusculas(string entrada, string esperado) => Pessoa.NormalizarEmail(entrada).ShouldBe(esperado);
}
