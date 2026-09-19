using Shared.Data.Entidades;

namespace Module.Palestras.Domain;

/// <summary>Vínculo entre uma palestra e uma pessoa (módulo Pessoas) com o papel exercido.</summary>
public sealed class PalestraPalestrante : EntidadeBase
{
    private PalestraPalestrante()
    {
    }

    internal PalestraPalestrante(Guid palestraId, Guid pessoaId, PalestrantePapel palestrantePapel)
    {
        PalestraId = palestraId;
        PessoaId = pessoaId;
        PalestrantePapel = palestrantePapel;
    }

    public Guid PalestraId { get; private set; }
    public Guid PessoaId { get; private set; }
    public PalestrantePapel PalestrantePapel { get; private set; }
}
