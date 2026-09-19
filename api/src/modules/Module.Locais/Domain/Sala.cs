using Shared.Data.Entidades;

namespace Module.Locais.Domain;

/// <summary>Ambiente de um local: auditório, sala de aula, área de recreação etc. É a unidade alocável para palestras.</summary>
public sealed class Sala : EntidadeBase
{
    private Sala()
    {
    }

    internal Sala(Guid localId, string salaNome, int salaCapacidade, SalaTipo salaTipo, string? salaRecursos)
    {
        LocalId = localId;
        SalaNome = salaNome.Trim();
        SalaCapacidade = salaCapacidade;
        SalaTipo = salaTipo;
        SalaRecursos = salaRecursos?.Trim();
    }

    public Guid LocalId { get; private set; }
    public string SalaNome { get; private set; } = string.Empty;
    public int SalaCapacidade { get; private set; }
    public SalaTipo SalaTipo { get; private set; }

    /// <summary>Descrição livre de recursos (ex.: "projetor, ar-condicionado, 40 tomadas").</summary>
    public string? SalaRecursos { get; private set; }

    public Local Local { get; private set; } = null!;

    internal void Atualizar(string salaNome, int salaCapacidade, SalaTipo salaTipo, string? salaRecursos)
    {
        SalaNome = salaNome.Trim();
        SalaCapacidade = salaCapacidade;
        SalaTipo = salaTipo;
        SalaRecursos = salaRecursos?.Trim();
    }
}
