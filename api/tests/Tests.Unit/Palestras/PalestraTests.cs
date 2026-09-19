using Module.Palestras.Domain;
using Shared.Contracts.Palestras;
using Shouldly;

namespace Tests.Unit.Palestras;

public class PalestraTests
{
    private static readonly DateTimeOffset Inicio = new(2026, 10, 1, 9, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset Fim = Inicio.AddMinutes(90);

    private static Palestra NovaPalestra(params Guid[] pessoaIds)
    {
        var palestrantes = pessoaIds.Select(id => new NovoPalestrante(id, PalestrantePapel.Principal)).ToList();
        var resultado = Palestra.Criar(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "  Monolito modular  ", null, Inicio, Fim, palestrantes);
        resultado.IsSuccess.ShouldBeTrue();
        return resultado.Value;
    }

    [Fact]
    public void Criar_deve_registrar_palestrantes_titulo_normalizado_e_evento_PalestraCriada()
    {
        var pessoa = Guid.NewGuid();
        var palestra = NovaPalestra(pessoa);

        palestra.PalestraTitulo.ShouldBe("Monolito modular");
        palestra.PalestraCargaHorariaMinutos.ShouldBe(90);
        palestra.Palestrantes.ShouldHaveSingleItem().PessoaId.ShouldBe(pessoa);
        palestra.Eventos.ShouldHaveSingleItem().ShouldBeOfType<PalestraCriada>().PalestraId.ShouldBe(palestra.Id);
    }

    [Fact]
    public void Criar_sem_palestrantes_deve_falhar()
    {
        var resultado = Palestra.Criar(Guid.NewGuid(), Guid.NewGuid(), null, "Título", null, Inicio, Fim, []);

        resultado.IsFailure.ShouldBeTrue();
        resultado.Error.ShouldBe(PalestrasErros.PalestraPrecisaDePalestrante);
    }

    [Fact]
    public void Criar_com_pessoa_repetida_deve_retornar_PalestranteJaVinculado()
    {
        var pessoa = Guid.NewGuid();
        var resultado = Palestra.Criar(Guid.NewGuid(), Guid.NewGuid(), null, "Título", null, Inicio, Fim,
            [new NovoPalestrante(pessoa, PalestrantePapel.Principal), new NovoPalestrante(pessoa, PalestrantePapel.Coautor)]);

        resultado.IsFailure.ShouldBeTrue();
        resultado.Error.ShouldBe(PalestrasErros.PalestranteJaVinculado);
    }

    [Fact]
    public void AdicionarPalestrante_repetido_deve_retornar_PalestranteJaVinculado()
    {
        var pessoa = Guid.NewGuid();
        var palestra = NovaPalestra(pessoa);

        var resultado = palestra.AdicionarPalestrante(pessoa, PalestrantePapel.Mediador);

        resultado.Error.ShouldBe(PalestrasErros.PalestranteJaVinculado);
    }

    [Fact]
    public void RemoverPalestrante_nao_deve_remover_o_ultimo()
    {
        var pessoa = Guid.NewGuid();
        var palestra = NovaPalestra(pessoa);

        var resultado = palestra.RemoverPalestrante(pessoa);

        resultado.IsFailure.ShouldBeTrue();
        resultado.Error.ShouldBe(PalestrasErros.PalestraPrecisaDePalestrante);
    }

    [Fact]
    public void RemoverPalestrante_com_mais_de_um_deve_retornar_o_vinculo()
    {
        var pessoa1 = Guid.NewGuid();
        var pessoa2 = Guid.NewGuid();
        var palestra = NovaPalestra(pessoa1, pessoa2);

        var resultado = palestra.RemoverPalestrante(pessoa2);

        resultado.IsSuccess.ShouldBeTrue();
        resultado.Value.PessoaId.ShouldBe(pessoa2);
    }

    [Fact]
    public void RemoverPalestrante_inexistente_deve_retornar_PalestranteNaoEncontrado()
    {
        var palestra = NovaPalestra(Guid.NewGuid(), Guid.NewGuid());

        palestra.RemoverPalestrante(Guid.NewGuid()).Error.ShouldBe(PalestrasErros.PalestranteNaoEncontrado);
    }

    [Fact]
    public void Conteudos_adicionar_e_remover()
    {
        var palestra = NovaPalestra(Guid.NewGuid());

        var conteudo = palestra.AdicionarConteudo(" Slides ", ConteudoTipo.Slides, "https://exemplo.com/slides.pdf ", null);
        conteudo.ConteudoTitulo.ShouldBe("Slides");
        conteudo.ConteudoUrl.ShouldBe("https://exemplo.com/slides.pdf");
        palestra.Conteudos.ShouldHaveSingleItem();

        palestra.RemoverConteudo(conteudo.Id).IsSuccess.ShouldBeTrue();
        palestra.RemoverConteudo(Guid.NewGuid()).Error.ShouldBe(PalestrasErros.ConteudoNaoEncontrado);
    }

    [Fact]
    public void RegistrarPresenca_deve_ser_unica_e_emitir_PresencaRegistrada()
    {
        var palestra = NovaPalestra(Guid.NewGuid());
        palestra.LimparEventos();
        var participante = Guid.NewGuid();

        var primeira = palestra.RegistrarPresenca(participante, Inicio.AddMinutes(5));
        var segunda = palestra.RegistrarPresenca(participante, Inicio.AddMinutes(6));

        primeira.IsSuccess.ShouldBeTrue();
        primeira.Value.PresencaRegistradaEm.ShouldBe(Inicio.AddMinutes(5));
        segunda.Error.ShouldBe(PalestrasErros.PresencaJaRegistrada);
        palestra.Presencas.ShouldHaveSingleItem();
        palestra.Eventos.ShouldHaveSingleItem().ShouldBeOfType<PresencaRegistrada>().PessoaId.ShouldBe(participante);
    }

    [Fact]
    public void EmitirCertificado_sem_presenca_deve_retornar_PresencaNaoRegistrada()
    {
        var palestra = NovaPalestra(Guid.NewGuid());

        palestra.EmitirCertificado(Guid.NewGuid(), Fim.AddHours(1)).Error.ShouldBe(PalestrasErros.PresencaNaoRegistrada);
    }

    [Fact]
    public void EmitirCertificado_antes_do_fim_deve_retornar_PalestraNaoEncerrada()
    {
        var palestra = NovaPalestra(Guid.NewGuid());
        var participante = Guid.NewGuid();
        palestra.RegistrarPresenca(participante, Inicio).IsSuccess.ShouldBeTrue();

        palestra.EmitirCertificado(participante, Fim.AddMinutes(-1)).Error.ShouldBe(PalestrasErros.PalestraNaoEncerrada);
        palestra.Certificados.ShouldBeEmpty();
    }

    [Fact]
    public void EmitirCertificado_apos_o_fim_deve_criar_com_codigo_e_carga_horaria_e_emitir_evento()
    {
        var palestra = NovaPalestra(Guid.NewGuid());
        var participante = Guid.NewGuid();
        palestra.RegistrarPresenca(participante, Inicio);
        palestra.LimparEventos();

        var resultado = palestra.EmitirCertificado(participante, Fim);

        resultado.IsSuccess.ShouldBeTrue();
        resultado.Value.Criado.ShouldBeTrue();
        resultado.Value.Certificado.CertificadoCodigo.Length.ShouldBe(CertificadoCodigoGerador.Tamanho);
        resultado.Value.Certificado.CertificadoCargaHorariaMinutos.ShouldBe(90);
        resultado.Value.Certificado.CertificadoEmitidoEm.ShouldBe(Fim);
        palestra.Eventos.ShouldHaveSingleItem().ShouldBeOfType<CertificadoEmitido>().CertificadoCodigo.ShouldBe(resultado.Value.Certificado.CertificadoCodigo);
    }

    [Fact]
    public void EmitirCertificado_deve_ser_idempotente()
    {
        var palestra = NovaPalestra(Guid.NewGuid());
        var participante = Guid.NewGuid();
        palestra.RegistrarPresenca(participante, Inicio);

        var primeira = palestra.EmitirCertificado(participante, Fim.AddHours(1));
        palestra.LimparEventos();
        var segunda = palestra.EmitirCertificado(participante, Fim.AddHours(2));

        primeira.Value.Criado.ShouldBeTrue();
        segunda.Value.Criado.ShouldBeFalse();
        segunda.Value.Certificado.ShouldBeSameAs(primeira.Value.Certificado);
        palestra.Certificados.ShouldHaveSingleItem();
        palestra.Eventos.ShouldBeEmpty();
    }
}
