using Shared.Contracts.Eventos;
using Shared.Data.Entidades;
using Shared.Http.Results;

namespace Module.Eventos.Domain;

/// <summary>
/// Agregado Evento: dados básicos, formato (presencial/remoto/híbrido), máquina de estados da situação e inscrições.
/// Toda regra de negócio devolve <see cref="Result"/>; eventos de integração são registrados aqui e gravados no Outbox pelo contexto.
/// </summary>
public sealed class Evento : EntidadeBase
{
    private readonly List<Inscricao> _inscricoes = [];
    private readonly List<Trilha> _trilhas = [];

    private Evento()
    {
    }

    public string EventoNome { get; private set; } = string.Empty;
    public string? EventoDescricao { get; private set; }
    public DateTimeOffset EventoDataInicio { get; private set; }
    public DateTimeOffset EventoDataFim { get; private set; }
    public EventoFormato EventoFormato { get; private set; }
    public Guid? LocalId { get; private set; }
    public string? EventoLinkRemoto { get; private set; }
    public EventoSituacao EventoSituacao { get; private set; }
    public int? EventoCapacidadeMaxima { get; private set; }
    public string? EventoCancelamentoMotivo { get; private set; }

    public IReadOnlyCollection<Inscricao> Inscricoes => _inscricoes.AsReadOnly();
    public IReadOnlyCollection<Trilha> Trilhas => _trilhas.AsReadOnly();

    public Result<Trilha> AdicionarTrilha(string trilhaNome, string? trilhaDescricao, string? trilhaCor)
    {
        if (_trilhas.Any(t => t.ExcluidoEm is null && string.Equals(t.TrilhaNome, trilhaNome.Trim(), StringComparison.OrdinalIgnoreCase)))
            return EventosErros.TrilhaNomeDuplicado;

        var trilha = new Trilha(Id, trilhaNome, trilhaDescricao, trilhaCor);
        _trilhas.Add(trilha);
        return trilha;
    }

    public Result AtualizarTrilha(Guid trilhaId, string trilhaNome, string? trilhaDescricao, string? trilhaCor)
    {
        var trilha = _trilhas.FirstOrDefault(t => t.Id == trilhaId && t.ExcluidoEm is null);
        if (trilha is null) return EventosErros.TrilhaNaoEncontrada;
        if (_trilhas.Any(t => t.Id != trilhaId && t.ExcluidoEm is null && string.Equals(t.TrilhaNome, trilhaNome.Trim(), StringComparison.OrdinalIgnoreCase)))
            return EventosErros.TrilhaNomeDuplicado;
        trilha.Atualizar(trilhaNome, trilhaDescricao, trilhaCor);
        return Result.Success();
    }

    public Result<Trilha> RemoverTrilha(Guid trilhaId)
    {
        var trilha = _trilhas.FirstOrDefault(t => t.Id == trilhaId && t.ExcluidoEm is null);
        if (trilha is null) return EventosErros.TrilhaNaoEncontrada;
        if (_trilhas.Count(t => t.ExcluidoEm is null) <= 1) return EventosErros.EventoPrecisaDeUmaTrilha;
        return trilha;
    }

    /// <summary>Evento nasce em <see cref="EventoSituacao.Rascunho"/>. Falha com <c>FormatoInconsistente</c> se local/link não combinam com o formato.</summary>
    public static Result<Evento> Criar(
        string eventoNome,
        string? eventoDescricao,
        DateTimeOffset eventoDataInicio,
        DateTimeOffset eventoDataFim,
        EventoFormato eventoFormato,
        Guid? localId,
        string? eventoLinkRemoto,
        int? eventoCapacidadeMaxima)
    {
        var evento = new Evento { EventoSituacao = EventoSituacao.Rascunho };
        var resultado = evento.AplicarDados(eventoNome, eventoDescricao, eventoDataInicio, eventoDataFim, eventoFormato, localId, eventoLinkRemoto, eventoCapacidadeMaxima);
        return resultado.IsFailure ? resultado.Error : evento;
    }

    /// <summary>Atualização permitida apenas em Rascunho ou Publicado (<c>EventoNaoPodeSerAlterado</c>).</summary>
    public Result Atualizar(
        string eventoNome,
        string? eventoDescricao,
        DateTimeOffset eventoDataInicio,
        DateTimeOffset eventoDataFim,
        EventoFormato eventoFormato,
        Guid? localId,
        string? eventoLinkRemoto,
        int? eventoCapacidadeMaxima)
    {
        if (!PodeSerAlterado)
        {
            return EventosErros.EventoNaoPodeSerAlterado;
        }

        return AplicarDados(eventoNome, eventoDescricao, eventoDataInicio, eventoDataFim, eventoFormato, localId, eventoLinkRemoto, eventoCapacidadeMaxima);
    }

    public bool PodeSerAlterado => EventoSituacao is EventoSituacao.Rascunho or EventoSituacao.Publicado;

    public bool PodeSerExcluido => EventoSituacao is EventoSituacao.Rascunho or EventoSituacao.Cancelado;

    public bool AceitaInscricoes => EventoSituacao is EventoSituacao.Publicado or EventoSituacao.EmAndamento;

    /// <summary>Rascunho → Publicado. Exige ao menos uma palestra (contada pelo módulo Palestras). Emite <see cref="EventoPublicado"/>.</summary>
    public Result Publicar(int quantidadePalestras)
    {
        if (EventoSituacao != EventoSituacao.Rascunho)
        {
            return EventosErros.TransicaoSituacaoInvalida;
        }

        if (quantidadePalestras < 1)
        {
            return EventosErros.EventoSemPalestras;
        }

        EventoSituacao = EventoSituacao.Publicado;
        RegistrarEvento(new EventoPublicado(Id, EventoNome, EventoDataInicio));
        return Result.Success();
    }

    /// <summary>Publicado → EmAndamento.</summary>
    public Result Iniciar()
    {
        if (EventoSituacao != EventoSituacao.Publicado)
        {
            return EventosErros.TransicaoSituacaoInvalida;
        }

        EventoSituacao = EventoSituacao.EmAndamento;
        return Result.Success();
    }

    /// <summary>Publicado|EmAndamento → Encerrado.</summary>
    public Result Encerrar()
    {
        if (EventoSituacao is not (EventoSituacao.Publicado or EventoSituacao.EmAndamento))
        {
            return EventosErros.TransicaoSituacaoInvalida;
        }

        EventoSituacao = EventoSituacao.Encerrado;
        return Result.Success();
    }

    /// <summary>Rascunho|Publicado|EmAndamento → Cancelado, com motivo obrigatório. Emite <see cref="EventoCancelado"/>.</summary>
    public Result Cancelar(string? motivo)
    {
        if (EventoSituacao is not (EventoSituacao.Rascunho or EventoSituacao.Publicado or EventoSituacao.EmAndamento))
        {
            return EventosErros.TransicaoSituacaoInvalida;
        }

        if (string.IsNullOrWhiteSpace(motivo))
        {
            return EventosErros.MotivoCancelamentoObrigatorio;
        }

        EventoSituacao = EventoSituacao.Cancelado;
        EventoCancelamentoMotivo = motivo.Trim();
        RegistrarEvento(new EventoCancelado(Id, EventoNome, EventoCancelamentoMotivo));
        return Result.Success();
    }

    /// <summary>Exclusão lógica permitida apenas em Rascunho ou Cancelado (<c>EventoNaoPodeSerExcluido</c>).</summary>
    public Result MarcarExcluido() => PodeSerExcluido ? Result.Success() : EventosErros.EventoNaoPodeSerExcluido;

    /// <summary>
    /// Capacidade efetiva: <see cref="EventoCapacidadeMaxima"/> ou, quando nula e há local, a capacidade total do local.
    /// <c>null</c> significa sem limite.
    /// </summary>
    public int? CapacidadeEfetiva(int? localCapacidadeTotal) =>
        EventoCapacidadeMaxima ?? (LocalId.HasValue ? localCapacidadeTotal : null);

    /// <summary>
    /// Inscreve uma pessoa. <paramref name="inscricoesConfirmadas"/> é a contagem atual (consulta projetada);
    /// <paramref name="localCapacidadeTotal"/> vem do <c>LocalResumo</c> quando há local. Emite <see cref="InscricaoRealizada"/>.
    /// A unicidade por pessoa é garantida também por índice único filtrado no banco.
    /// </summary>
    public Result<Inscricao> Inscrever(Guid pessoaId, int inscricoesConfirmadas, int? localCapacidadeTotal, DateTimeOffset agora)
    {
        if (!AceitaInscricoes)
        {
            return EventosErros.EventoNaoAceitaInscricoes;
        }

        if (_inscricoes.Any(i => i.PessoaId == pessoaId && i.EstaConfirmada))
        {
            return EventosErros.PessoaJaInscrita;
        }

        var capacidade = CapacidadeEfetiva(localCapacidadeTotal);
        if (capacidade.HasValue && inscricoesConfirmadas >= capacidade.Value)
        {
            return EventosErros.CapacidadeEsgotada;
        }

        var inscricao = new Inscricao(Id, pessoaId, agora);
        _inscricoes.Add(inscricao);
        RegistrarEvento(new InscricaoRealizada(inscricao.Id, Id, pessoaId));
        return inscricao;
    }

    /// <summary>Cancela uma inscrição carregada na coleção (não é soft delete). Emite <see cref="InscricaoCancelada"/>.</summary>
    public Result<Inscricao> CancelarInscricao(Guid inscricaoId, DateTimeOffset agora)
    {
        var inscricao = _inscricoes.FirstOrDefault(i => i.Id == inscricaoId);
        if (inscricao is null)
        {
            return EventosErros.InscricaoNaoEncontrada;
        }

        if (!inscricao.EstaConfirmada)
        {
            return EventosErros.InscricaoJaCancelada;
        }

        inscricao.Cancelar(agora);
        RegistrarEvento(new InscricaoCancelada(inscricao.Id, Id, inscricao.PessoaId));
        return inscricao;
    }

    /// <summary>Consistência entre formato e dados de local/link: Presencial exige local; Remoto exige link e não admite local; Híbrido exige ambos.</summary>
    public static bool FormatoConsistente(EventoFormato formato, Guid? localId, string? eventoLinkRemoto)
    {
        var temLocal = localId.HasValue && localId.Value != Guid.Empty;
        var temLink = !string.IsNullOrWhiteSpace(eventoLinkRemoto);
        return formato switch
        {
            EventoFormato.Presencial => temLocal,
            EventoFormato.Remoto => temLink && !temLocal,
            EventoFormato.Hibrido => temLocal && temLink,
            _ => false,
        };
    }

    private Result AplicarDados(
        string eventoNome,
        string? eventoDescricao,
        DateTimeOffset eventoDataInicio,
        DateTimeOffset eventoDataFim,
        EventoFormato eventoFormato,
        Guid? localId,
        string? eventoLinkRemoto,
        int? eventoCapacidadeMaxima)
    {
        if (!FormatoConsistente(eventoFormato, localId, eventoLinkRemoto))
        {
            return EventosErros.FormatoInconsistente;
        }

        EventoNome = eventoNome.Trim();
        EventoDescricao = string.IsNullOrWhiteSpace(eventoDescricao) ? null : eventoDescricao.Trim();
        EventoDataInicio = eventoDataInicio;
        EventoDataFim = eventoDataFim;
        EventoFormato = eventoFormato;
        LocalId = localId;
        EventoLinkRemoto = string.IsNullOrWhiteSpace(eventoLinkRemoto) ? null : eventoLinkRemoto.Trim();
        EventoCapacidadeMaxima = eventoCapacidadeMaxima;
        return Result.Success();
    }
}
