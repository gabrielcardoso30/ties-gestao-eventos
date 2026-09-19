using Shared.Data.Entidades;

namespace Module.Eventos.Domain;

/// <summary>Inscrição de uma pessoa em um evento. Entidade filha do agregado <see cref="Evento"/>.</summary>
public sealed class Inscricao : EntidadeBase
{
    private Inscricao()
    {
    }

    internal Inscricao(Guid eventoId, Guid pessoaId, DateTimeOffset realizadaEm)
    {
        EventoId = eventoId;
        PessoaId = pessoaId;
        InscricaoSituacao = InscricaoSituacao.Confirmada;
        InscricaoRealizadaEm = realizadaEm;
    }

    public Guid EventoId { get; private set; }
    public Guid PessoaId { get; private set; }
    public InscricaoSituacao InscricaoSituacao { get; private set; }
    public DateTimeOffset InscricaoRealizadaEm { get; private set; }
    public DateTimeOffset? InscricaoCanceladaEm { get; private set; }

    public Evento Evento { get; private set; } = null!;

    public bool EstaConfirmada => InscricaoSituacao == InscricaoSituacao.Confirmada;

    internal void Cancelar(DateTimeOffset canceladaEm)
    {
        InscricaoSituacao = InscricaoSituacao.Cancelada;
        InscricaoCanceladaEm = canceladaEm;
    }
}
