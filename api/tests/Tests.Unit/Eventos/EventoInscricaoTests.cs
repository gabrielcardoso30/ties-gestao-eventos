using Module.Eventos.Domain;
using NSubstitute;
using Shared.Contracts.Eventos;
using Shared.Contracts.Locais;
using Shouldly;
using Xunit;

namespace Tests.Unit.Eventos;

public sealed class EventoInscricaoTests
{
    private static readonly DateTimeOffset Agora = new(2026, 9, 18, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Inscrever_em_publicado_deve_confirmar_e_emitir_InscricaoRealizada()
    {
        var evento = EventoFabrica.NaSituacao(EventoSituacao.Publicado);
        var pessoaId = Guid.NewGuid();

        var resultado = evento.Inscrever(pessoaId, inscricoesConfirmadas: 0, localCapacidadeTotal: 100, Agora);

        resultado.IsSuccess.ShouldBeTrue();
        var inscricao = resultado.Value;
        inscricao.EventoId.ShouldBe(evento.Id);
        inscricao.PessoaId.ShouldBe(pessoaId);
        inscricao.InscricaoSituacao.ShouldBe(InscricaoSituacao.Confirmada);
        inscricao.InscricaoRealizadaEm.ShouldBe(Agora);
        inscricao.InscricaoCanceladaEm.ShouldBeNull();
        evento.Inscricoes.ShouldHaveSingleItem().ShouldBe(inscricao);
        var integracao = evento.Eventos.ShouldHaveSingleItem().ShouldBeOfType<InscricaoRealizada>();
        integracao.InscricaoId.ShouldBe(inscricao.Id);
        integracao.EventoId.ShouldBe(evento.Id);
        integracao.PessoaId.ShouldBe(pessoaId);
    }

    [Fact]
    public void Inscrever_em_andamento_deve_ser_permitido()
    {
        var evento = EventoFabrica.NaSituacao(EventoSituacao.EmAndamento);

        evento.Inscrever(Guid.NewGuid(), 0, null, Agora).IsSuccess.ShouldBeTrue();
    }

    [Theory]
    [InlineData(EventoSituacao.Rascunho)]
    [InlineData(EventoSituacao.Encerrado)]
    [InlineData(EventoSituacao.Cancelado)]
    public void Inscrever_fora_de_publicado_ou_em_andamento_deve_falhar(EventoSituacao atual)
    {
        var evento = EventoFabrica.NaSituacao(atual);

        var resultado = evento.Inscrever(Guid.NewGuid(), 0, null, Agora);

        resultado.Error.ShouldBe(EventosErros.EventoNaoAceitaInscricoes);
        evento.Inscricoes.ShouldBeEmpty();
    }

    [Fact]
    public void Inscrever_deve_respeitar_capacidade_maxima_do_evento()
    {
        var evento = EventoFabrica.NaSituacao(EventoSituacao.Publicado, capacidade: 2);

        evento.Inscrever(Guid.NewGuid(), inscricoesConfirmadas: 1, localCapacidadeTotal: 1000, Agora).IsSuccess.ShouldBeTrue();
        var esgotado = evento.Inscrever(Guid.NewGuid(), inscricoesConfirmadas: 2, localCapacidadeTotal: 1000, Agora);

        esgotado.Error.ShouldBe(EventosErros.CapacidadeEsgotada);
    }

    [Fact]
    public void Inscrever_sem_capacidade_maxima_deve_usar_capacidade_do_local()
    {
        var evento = EventoFabrica.NaSituacao(EventoSituacao.Publicado, capacidade: null);
        var locais = Substitute.For<ILocaisModuleApi>();
        locais.ObterLocalResumoAsync(EventoFabrica.LocalId, Arg.Any<CancellationToken>())
            .Returns(new LocalResumo(EventoFabrica.LocalId, "Auditório", LocalCapacidadeTotal: 3, SalasQuantidade: 1));
        var local = locais.ObterLocalResumoAsync(EventoFabrica.LocalId, CancellationToken.None).Result;

        evento.CapacidadeEfetiva(local!.LocalCapacidadeTotal).ShouldBe(3);
        evento.Inscrever(Guid.NewGuid(), inscricoesConfirmadas: 2, local.LocalCapacidadeTotal, Agora).IsSuccess.ShouldBeTrue();
        evento.Inscrever(Guid.NewGuid(), inscricoesConfirmadas: 3, local.LocalCapacidadeTotal, Agora).Error.ShouldBe(EventosErros.CapacidadeEsgotada);
    }

    [Fact]
    public void Capacidade_maxima_do_evento_prevalece_sobre_a_do_local()
    {
        var evento = EventoFabrica.NaSituacao(EventoSituacao.Publicado, capacidade: 5);

        evento.CapacidadeEfetiva(localCapacidadeTotal: 500).ShouldBe(5);
    }

    [Fact]
    public void Evento_remoto_sem_capacidade_maxima_nao_tem_limite()
    {
        var evento = EventoFabrica.Remoto();
        evento.Publicar(1);

        evento.CapacidadeEfetiva(localCapacidadeTotal: null).ShouldBeNull();
        evento.Inscrever(Guid.NewGuid(), inscricoesConfirmadas: 10_000, null, Agora).IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public void Inscrever_pessoa_ja_confirmada_na_colecao_deve_falhar_com_PessoaJaInscrita()
    {
        var evento = EventoFabrica.NaSituacao(EventoSituacao.Publicado);
        var pessoaId = Guid.NewGuid();
        evento.Inscrever(pessoaId, 0, null, Agora);

        var resultado = evento.Inscrever(pessoaId, 1, null, Agora);

        resultado.Error.ShouldBe(EventosErros.PessoaJaInscrita);
        evento.Inscricoes.Count.ShouldBe(1);
    }

    [Fact]
    public void Inscrever_pessoa_com_inscricao_cancelada_deve_permitir_nova_inscricao()
    {
        var evento = EventoFabrica.NaSituacao(EventoSituacao.Publicado);
        var pessoaId = Guid.NewGuid();
        var primeira = evento.Inscrever(pessoaId, 0, null, Agora).Value;
        evento.CancelarInscricao(primeira.Id, Agora.AddMinutes(1));

        var segunda = evento.Inscrever(pessoaId, 0, null, Agora.AddMinutes(2));

        segunda.IsSuccess.ShouldBeTrue();
        evento.Inscricoes.Count.ShouldBe(2);
    }

    [Fact]
    public void CancelarInscricao_deve_mudar_situacao_e_emitir_InscricaoCancelada()
    {
        var evento = EventoFabrica.NaSituacao(EventoSituacao.Publicado);
        var pessoaId = Guid.NewGuid();
        var inscricao = evento.Inscrever(pessoaId, 0, null, Agora).Value;
        evento.LimparEventos();
        var canceladaEm = Agora.AddHours(1);

        var resultado = evento.CancelarInscricao(inscricao.Id, canceladaEm);

        resultado.IsSuccess.ShouldBeTrue();
        inscricao.InscricaoSituacao.ShouldBe(InscricaoSituacao.Cancelada);
        inscricao.InscricaoCanceladaEm.ShouldBe(canceladaEm);
        inscricao.ExcluidoEm.ShouldBeNull();
        var integracao = evento.Eventos.ShouldHaveSingleItem().ShouldBeOfType<InscricaoCancelada>();
        integracao.InscricaoId.ShouldBe(inscricao.Id);
        integracao.PessoaId.ShouldBe(pessoaId);
    }

    [Fact]
    public void CancelarInscricao_ja_cancelada_deve_falhar_com_InscricaoJaCancelada()
    {
        var evento = EventoFabrica.NaSituacao(EventoSituacao.Publicado);
        var inscricao = evento.Inscrever(Guid.NewGuid(), 0, null, Agora).Value;
        evento.CancelarInscricao(inscricao.Id, Agora);

        var resultado = evento.CancelarInscricao(inscricao.Id, Agora);

        resultado.Error.ShouldBe(EventosErros.InscricaoJaCancelada);
    }

    [Fact]
    public void CancelarInscricao_inexistente_deve_falhar_com_InscricaoNaoEncontrada()
    {
        var evento = EventoFabrica.NaSituacao(EventoSituacao.Publicado);

        evento.CancelarInscricao(Guid.NewGuid(), Agora).Error.ShouldBe(EventosErros.InscricaoNaoEncontrada);
    }
}
