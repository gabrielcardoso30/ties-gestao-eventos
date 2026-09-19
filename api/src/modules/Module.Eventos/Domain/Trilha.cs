using Shared.Data.Entidades;

namespace Module.Eventos.Domain;

/// <summary>Trilha temática que organiza as palestras de um evento.</summary>
public sealed class Trilha : EntidadeBase
{
    private Trilha() { }

    internal Trilha(Guid eventoId, string trilhaNome, string? trilhaDescricao, string? trilhaCor)
    {
        EventoId = eventoId;
        Atualizar(trilhaNome, trilhaDescricao, trilhaCor);
    }

    public Guid EventoId { get; private set; }
    public string TrilhaNome { get; private set; } = string.Empty;
    public string? TrilhaDescricao { get; private set; }
    public string? TrilhaCor { get; private set; }
    public Evento Evento { get; private set; } = null!;

    internal void Atualizar(string trilhaNome, string? trilhaDescricao, string? trilhaCor)
    {
        TrilhaNome = trilhaNome.Trim();
        TrilhaDescricao = string.IsNullOrWhiteSpace(trilhaDescricao) ? null : trilhaDescricao.Trim();
        TrilhaCor = string.IsNullOrWhiteSpace(trilhaCor) ? null : trilhaCor.Trim().ToUpperInvariant();
    }
}
