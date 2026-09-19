using Module.Eventos.Domain;
using Module.Eventos.UseCases.AlterarSituacaoEvento;
using Module.Eventos.UseCases.AtualizarEvento;
using Module.Eventos.UseCases.CriarEvento;
using Module.Eventos.UseCases.InscreverParticipante;
using Module.Eventos.UseCases.ListarEventos;
using Module.Eventos.UseCases.ListarInscricoes;
using Shouldly;
using Xunit;

namespace Tests.Unit.Eventos;

public sealed class CriarEventoValidatorTests
{
    private readonly CriarEventoValidator _validator = new();
    private static readonly Guid Local = Guid.NewGuid();
    private const string Link = "https://meet.exemplo.com/sala";

    private static CriarEventoRequest Valido(EventoFormato formato = EventoFormato.Presencial, Guid? localId = null, string? link = null) =>
        new("DevConf", "Descrição", EventoFabrica.Inicio, EventoFabrica.Fim, formato,
            localId ?? (formato == EventoFormato.Remoto ? null : Local),
            link ?? (formato == EventoFormato.Presencial ? null : Link), 100);

    [Theory]
    [InlineData(EventoFormato.Presencial)]
    [InlineData(EventoFormato.Remoto)]
    [InlineData(EventoFormato.Hibrido)]
    public void Request_consistente_deve_ser_valido(EventoFormato formato)
    {
        _validator.Validate(Valido(formato)).IsValid.ShouldBeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Nome_obrigatorio(string nome)
    {
        var resultado = _validator.Validate(Valido() with { EventoNome = nome });

        resultado.IsValid.ShouldBeFalse();
        resultado.Errors.ShouldContain(e => e.PropertyName == nameof(CriarEventoRequest.EventoNome));
    }

    [Fact]
    public void Nome_com_mais_de_200_caracteres_invalido()
    {
        _validator.Validate(Valido() with { EventoNome = new string('a', 201) }).IsValid.ShouldBeFalse();
    }

    [Fact]
    public void Descricao_com_mais_de_4000_caracteres_invalida()
    {
        _validator.Validate(Valido() with { EventoDescricao = new string('a', 4001) }).IsValid.ShouldBeFalse();
    }

    [Fact]
    public void DataFim_deve_ser_posterior_a_DataInicio()
    {
        var igual = _validator.Validate(Valido() with { EventoDataFim = EventoFabrica.Inicio });
        var anterior = _validator.Validate(Valido() with { EventoDataFim = EventoFabrica.Inicio.AddHours(-1) });

        igual.Errors.ShouldContain(e => e.PropertyName == nameof(CriarEventoRequest.EventoDataFim));
        anterior.Errors.ShouldContain(e => e.PropertyName == nameof(CriarEventoRequest.EventoDataFim));
    }

    [Fact]
    public void Formato_fora_do_enum_invalido()
    {
        _validator.Validate(Valido() with { EventoFormato = (EventoFormato)99 }).IsValid.ShouldBeFalse();
    }

    [Fact]
    public void Presencial_sem_local_invalido()
    {
        var resultado = _validator.Validate(Valido(EventoFormato.Presencial) with { LocalId = null });

        resultado.Errors.ShouldContain(e => e.PropertyName == nameof(CriarEventoRequest.LocalId));
    }

    [Fact]
    public void Remoto_sem_link_invalido()
    {
        var resultado = _validator.Validate(Valido(EventoFormato.Remoto) with { EventoLinkRemoto = null });

        resultado.Errors.ShouldContain(e => e.PropertyName == nameof(CriarEventoRequest.EventoLinkRemoto));
    }

    [Fact]
    public void Remoto_com_local_invalido()
    {
        var resultado = _validator.Validate(Valido(EventoFormato.Remoto) with { LocalId = Local });

        resultado.Errors.ShouldContain(e => e.PropertyName == nameof(CriarEventoRequest.LocalId));
    }

    [Fact]
    public void Hibrido_sem_local_ou_sem_link_invalido()
    {
        _validator.Validate(Valido(EventoFormato.Hibrido) with { LocalId = null }).IsValid.ShouldBeFalse();
        _validator.Validate(Valido(EventoFormato.Hibrido) with { EventoLinkRemoto = "" }).IsValid.ShouldBeFalse();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Capacidade_deve_ser_maior_que_zero_quando_informada(int capacidade)
    {
        _validator.Validate(Valido() with { EventoCapacidadeMaxima = capacidade }).IsValid.ShouldBeFalse();
    }

    [Fact]
    public void Capacidade_nula_e_valida()
    {
        _validator.Validate(Valido() with { EventoCapacidadeMaxima = null }).IsValid.ShouldBeTrue();
    }

    [Fact]
    public void LocalId_vazio_invalido()
    {
        _validator.Validate(Valido() with { LocalId = Guid.Empty }).IsValid.ShouldBeFalse();
    }
}

public sealed class AtualizarEventoValidatorTests
{
    private readonly AtualizarEventoValidator _validator = new();
    private static readonly Guid Local = Guid.NewGuid();

    private static AtualizarEventoRequest Valido() =>
        new("DevConf", null, EventoFabrica.Inicio, EventoFabrica.Fim, EventoFormato.Presencial, Local, null, null) { EventoId = Guid.NewGuid() };

    [Fact]
    public void Request_consistente_deve_ser_valido() => _validator.Validate(Valido()).IsValid.ShouldBeTrue();

    [Fact]
    public void Nome_obrigatorio() => _validator.Validate(Valido() with { EventoNome = "" }).IsValid.ShouldBeFalse();

    [Fact]
    public void DataFim_anterior_invalida() =>
        _validator.Validate(Valido() with { EventoDataFim = EventoFabrica.Inicio.AddDays(-1) }).IsValid.ShouldBeFalse();

    [Fact]
    public void Remoto_com_local_invalido() =>
        _validator.Validate(Valido() with { EventoFormato = EventoFormato.Remoto, EventoLinkRemoto = "https://x" }).IsValid.ShouldBeFalse();

    [Fact]
    public void Remoto_sem_local_com_link_valido() =>
        _validator.Validate(Valido() with { EventoFormato = EventoFormato.Remoto, LocalId = null, EventoLinkRemoto = "https://x" }).IsValid.ShouldBeTrue();

    [Fact]
    public void Hibrido_sem_link_invalido() =>
        _validator.Validate(Valido() with { EventoFormato = EventoFormato.Hibrido }).IsValid.ShouldBeFalse();
}

public sealed class AlterarSituacaoEventoValidatorTests
{
    private readonly AlterarSituacaoEventoValidator _validator = new();

    [Theory]
    [InlineData(EventoSituacao.Publicado)]
    [InlineData(EventoSituacao.EmAndamento)]
    [InlineData(EventoSituacao.Encerrado)]
    [InlineData(EventoSituacao.Cancelado)]
    [InlineData(EventoSituacao.Rascunho)]
    public void Situacoes_do_enum_sao_validas_no_validator(EventoSituacao situacao)
    {
        // A validade da transição (inclusive voltar para Rascunho) é regra de domínio (422), não de validação (400).
        _validator.Validate(new AlterarSituacaoEventoRequest(situacao, null)).IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Situacao_fora_do_enum_invalida() =>
        _validator.Validate(new AlterarSituacaoEventoRequest((EventoSituacao)42, null)).IsValid.ShouldBeFalse();

    [Fact]
    public void Cancelar_sem_motivo_passa_no_validator_pois_o_dominio_responde_422()
    {
        _validator.Validate(new AlterarSituacaoEventoRequest(EventoSituacao.Cancelado, null)).IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Motivo_com_mais_de_1000_caracteres_invalido() =>
        _validator.Validate(new AlterarSituacaoEventoRequest(EventoSituacao.Cancelado, new string('m', 1001))).IsValid.ShouldBeFalse();
}

public sealed class InscreverParticipanteValidatorTests
{
    private readonly InscreverParticipanteValidator _validator = new();

    [Fact]
    public void PessoaId_obrigatorio() => _validator.Validate(new InscreverParticipanteRequest(Guid.Empty)).IsValid.ShouldBeFalse();

    [Fact]
    public void PessoaId_valido() => _validator.Validate(new InscreverParticipanteRequest(Guid.NewGuid())).IsValid.ShouldBeTrue();
}

public sealed class ListarEventosValidatorTests
{
    private readonly ListarEventosValidator _validator = new();

    [Fact]
    public void Padrao_valido() => _validator.Validate(new ListarEventosRequest(null, null, null, null, null)).IsValid.ShouldBeTrue();

    [Theory]
    [InlineData(0, 20)]
    [InlineData(1, 0)]
    [InlineData(1, 101)]
    public void Paginacao_fora_dos_limites_invalida(int pagina, int tamanho) =>
        _validator.Validate(new ListarEventosRequest(null, null, null, null, null, pagina, tamanho)).IsValid.ShouldBeFalse();

    [Fact]
    public void Busca_com_mais_de_100_caracteres_invalida() =>
        _validator.Validate(new ListarEventosRequest(new string('b', 101), null, null, null, null)).IsValid.ShouldBeFalse();

    [Fact]
    public void Intervalo_de_datas_invertido_invalido() =>
        _validator.Validate(new ListarEventosRequest(null, null, null, EventoFabrica.Fim, EventoFabrica.Inicio)).IsValid.ShouldBeFalse();

    [Fact]
    public void Filtros_por_enum_validos() =>
        _validator.Validate(new ListarEventosRequest("dev", EventoSituacao.Publicado, EventoFormato.Hibrido, EventoFabrica.Inicio, EventoFabrica.Fim)).IsValid.ShouldBeTrue();
}

public sealed class ListarInscricoesValidatorTests
{
    private readonly ListarInscricoesValidator _validator = new();

    [Fact]
    public void Padrao_valido() => _validator.Validate(new ListarInscricoesRequest(null)).IsValid.ShouldBeTrue();

    [Fact]
    public void Filtro_por_situacao_valido() => _validator.Validate(new ListarInscricoesRequest(InscricaoSituacao.Cancelada)).IsValid.ShouldBeTrue();

    [Fact]
    public void TamanhoPagina_acima_de_100_invalido() => _validator.Validate(new ListarInscricoesRequest(null, 1, 101)).IsValid.ShouldBeFalse();
}
