using Shared.Data.Entidades;

namespace Module.Palestras.Domain;

/// <summary>Registro de presença de uma pessoa em uma palestra. Única por (palestra, pessoa) entre ativos.</summary>
public sealed class Presenca : EntidadeBase
{
    private Presenca()
    {
    }

    internal Presenca(Guid palestraId, Guid pessoaId, DateTimeOffset presencaRegistradaEm)
    {
        PalestraId = palestraId;
        PessoaId = pessoaId;
        PresencaRegistradaEm = presencaRegistradaEm;
    }

    public Guid PalestraId { get; private set; }
    public Guid PessoaId { get; private set; }
    public DateTimeOffset PresencaRegistradaEm { get; private set; }
}
