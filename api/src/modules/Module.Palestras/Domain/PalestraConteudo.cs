using Shared.Data.Entidades;

namespace Module.Palestras.Domain;

/// <summary>Material da palestra: slides, PDF, link, vídeo etc.</summary>
public sealed class PalestraConteudo : EntidadeBase
{
    private PalestraConteudo()
    {
    }

    internal PalestraConteudo(Guid palestraId, string conteudoTitulo, ConteudoTipo conteudoTipo, string conteudoUrl, string? conteudoDescricao)
    {
        PalestraId = palestraId;
        ConteudoTitulo = conteudoTitulo.Trim();
        ConteudoTipo = conteudoTipo;
        ConteudoUrl = conteudoUrl.Trim();
        ConteudoDescricao = conteudoDescricao?.Trim();
    }

    public Guid PalestraId { get; private set; }
    public string ConteudoTitulo { get; private set; } = string.Empty;
    public ConteudoTipo ConteudoTipo { get; private set; }
    public string ConteudoUrl { get; private set; } = string.Empty;
    public string? ConteudoDescricao { get; private set; }
}
