using Module.Eventos.Domain;
using NSubstitute;
using Shared.Contracts.Eventos;
using Shared.Contracts.Palestras;
using Shouldly;
using Xunit;

namespace Tests.Unit.Eventos;

public sealed class EventoSituacaoTests
{
    [Fact]
    public void Criar_deve_nascer_em_rascunho_sem_eventos_de_integracao()
    {
        var evento = EventoFabrica.Presencial();

        evento.EventoSituacao.ShouldBe(EventoSituacao.Rascunho);
        evento.Eventos.ShouldBeEmpty();
    }

    [Fact]
    public void Publicar_em_rascunho_com_palestra_deve_publicar_e_emitir_EventoPublicado()
    {
        var evento = EventoFabrica.Presencial();

        var resultado = evento.Publicar(quantidadePalestras: 1);

        resultado.IsSuccess.ShouldBeTrue();
        evento.EventoSituacao.ShouldBe(EventoSituacao.Publicado);
        var integracao = evento.Eventos.ShouldHaveSingleItem().ShouldBeOfType<EventoPublicado>();
        integracao.EventoId.ShouldBe(evento.Id);
        integracao.EventoNome.ShouldBe("DevConf");
        integracao.EventoDataInicio.ShouldBe(EventoFabrica.Inicio);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Publicar_sem_palestras_deve_falhar_com_EventoSemPalestras(int quantidadePalestras)
    {
        var evento = EventoFabrica.Presencial();

        var resultado = evento.Publicar(quantidadePalestras);

        resultado.IsFailure.ShouldBeTrue();
        resultado.Error.ShouldBe(EventosErros.EventoSemPalestras);
        evento.EventoSituacao.ShouldBe(EventoSituacao.Rascunho);
        evento.Eventos.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(EventoSituacao.Publicado)]
    [InlineData(EventoSituacao.EmAndamento)]
    [InlineData(EventoSituacao.Encerrado)]
    [InlineData(EventoSituacao.Cancelado)]
    public void Publicar_fora_de_rascunho_deve_falhar_com_TransicaoSituacaoInvalida(EventoSituacao atual)
    {
        var evento = EventoFabrica.NaSituacao(atual);

        var resultado = evento.Publicar(5);

        resultado.Error.ShouldBe(EventosErros.TransicaoSituacaoInvalida);
        evento.EventoSituacao.ShouldBe(atual);
    }

    [Fact]
    public async Task Publicar_deve_usar_a_contagem_de_palestras_do_contrato_do_modulo_Palestras()
    {
        var evento = EventoFabrica.Presencial();
        var palestras = Substitute.For<IPalestrasModuleApi>();
        palestras.ContarPalestrasDoEventoAsync(evento.Id, Arg.Any<CancellationToken>()).Returns(2);

        var resultado = evento.Publicar(await palestras.ContarPalestrasDoEventoAsync(evento.Id, CancellationToken.None));

        resultado.IsSuccess.ShouldBeTrue();
        await palestras.Received(1).ContarPalestrasDoEventoAsync(evento.Id, Arg.Any<CancellationToken>());
    }

    [Fact]
    public void Iniciar_em_publicado_deve_ir_para_EmAndamento()
    {
        var evento = EventoFabrica.NaSituacao(EventoSituacao.Publicado);

        evento.Iniciar().IsSuccess.ShouldBeTrue();

        evento.EventoSituacao.ShouldBe(EventoSituacao.EmAndamento);
        evento.Eventos.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(EventoSituacao.Rascunho)]
    [InlineData(EventoSituacao.EmAndamento)]
    [InlineData(EventoSituacao.Encerrado)]
    [InlineData(EventoSituacao.Cancelado)]
    public void Iniciar_fora_de_publicado_deve_falhar(EventoSituacao atual)
    {
        var evento = EventoFabrica.NaSituacao(atual);

        evento.Iniciar().Error.ShouldBe(EventosErros.TransicaoSituacaoInvalida);

        evento.EventoSituacao.ShouldBe(atual);
    }

    [Theory]
    [InlineData(EventoSituacao.Publicado)]
    [InlineData(EventoSituacao.EmAndamento)]
    public void Encerrar_em_publicado_ou_em_andamento_deve_encerrar(EventoSituacao atual)
    {
        var evento = EventoFabrica.NaSituacao(atual);

        evento.Encerrar().IsSuccess.ShouldBeTrue();

        evento.EventoSituacao.ShouldBe(EventoSituacao.Encerrado);
    }

    [Theory]
    [InlineData(EventoSituacao.Rascunho)]
    [InlineData(EventoSituacao.Encerrado)]
    [InlineData(EventoSituacao.Cancelado)]
    public void Encerrar_fora_de_publicado_ou_em_andamento_deve_falhar(EventoSituacao atual)
    {
        var evento = EventoFabrica.NaSituacao(atual);

        evento.Encerrar().Error.ShouldBe(EventosErros.TransicaoSituacaoInvalida);

        evento.EventoSituacao.ShouldBe(atual);
    }

    [Theory]
    [InlineData(EventoSituacao.Rascunho)]
    [InlineData(EventoSituacao.Publicado)]
    [InlineData(EventoSituacao.EmAndamento)]
    public void Cancelar_com_motivo_deve_cancelar_e_emitir_EventoCancelado(EventoSituacao atual)
    {
        var evento = EventoFabrica.NaSituacao(atual);

        var resultado = evento.Cancelar("  Palestrante indisponível  ");

        resultado.IsSuccess.ShouldBeTrue();
        evento.EventoSituacao.ShouldBe(EventoSituacao.Cancelado);
        evento.EventoCancelamentoMotivo.ShouldBe("Palestrante indisponível");
        var integracao = evento.Eventos.ShouldHaveSingleItem().ShouldBeOfType<EventoCancelado>();
        integracao.EventoId.ShouldBe(evento.Id);
        integracao.Motivo.ShouldBe("Palestrante indisponível");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Cancelar_sem_motivo_deve_falhar_com_MotivoCancelamentoObrigatorio(string? motivo)
    {
        var evento = EventoFabrica.NaSituacao(EventoSituacao.Publicado);

        var resultado = evento.Cancelar(motivo);

        resultado.Error.ShouldBe(EventosErros.MotivoCancelamentoObrigatorio);
        evento.EventoSituacao.ShouldBe(EventoSituacao.Publicado);
        evento.Eventos.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(EventoSituacao.Encerrado)]
    [InlineData(EventoSituacao.Cancelado)]
    public void Cancelar_em_encerrado_ou_cancelado_deve_falhar(EventoSituacao atual)
    {
        var evento = EventoFabrica.NaSituacao(atual);

        evento.Cancelar("motivo").Error.ShouldBe(EventosErros.TransicaoSituacaoInvalida);

        evento.EventoSituacao.ShouldBe(atual);
    }

    [Theory]
    [InlineData(EventoSituacao.Rascunho, true)]
    [InlineData(EventoSituacao.Publicado, true)]
    [InlineData(EventoSituacao.EmAndamento, false)]
    [InlineData(EventoSituacao.Encerrado, false)]
    [InlineData(EventoSituacao.Cancelado, false)]
    public void Atualizar_somente_em_rascunho_ou_publicado(EventoSituacao atual, bool permitido)
    {
        var evento = EventoFabrica.NaSituacao(atual);

        var resultado = evento.Atualizar("Novo nome", null, EventoFabrica.Inicio, EventoFabrica.Fim, EventoFormato.Presencial, EventoFabrica.LocalId, null, 10);

        resultado.IsSuccess.ShouldBe(permitido);
        if (permitido)
        {
            evento.EventoNome.ShouldBe("Novo nome");
            evento.EventoCapacidadeMaxima.ShouldBe(10);
        }
        else
        {
            resultado.Error.ShouldBe(EventosErros.EventoNaoPodeSerAlterado);
            evento.EventoNome.ShouldBe("DevConf");
        }
    }

    [Theory]
    [InlineData(EventoSituacao.Rascunho, true)]
    [InlineData(EventoSituacao.Publicado, false)]
    [InlineData(EventoSituacao.EmAndamento, false)]
    [InlineData(EventoSituacao.Encerrado, false)]
    [InlineData(EventoSituacao.Cancelado, true)]
    public void Excluir_somente_em_rascunho_ou_cancelado(EventoSituacao atual, bool permitido)
    {
        var evento = EventoFabrica.NaSituacao(atual);

        var resultado = evento.MarcarExcluido();

        resultado.IsSuccess.ShouldBe(permitido);
        if (!permitido)
        {
            resultado.Error.ShouldBe(EventosErros.EventoNaoPodeSerExcluido);
        }
    }

    [Theory]
    [InlineData(EventoSituacao.Rascunho, false)]
    [InlineData(EventoSituacao.Publicado, true)]
    [InlineData(EventoSituacao.EmAndamento, true)]
    [InlineData(EventoSituacao.Encerrado, false)]
    [InlineData(EventoSituacao.Cancelado, false)]
    public void AceitaInscricoes_somente_em_publicado_ou_em_andamento(EventoSituacao atual, bool esperado)
    {
        EventoFabrica.NaSituacao(atual).AceitaInscricoes.ShouldBe(esperado);
    }
}
