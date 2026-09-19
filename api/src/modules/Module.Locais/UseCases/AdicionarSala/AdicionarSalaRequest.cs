using System.Text.Json.Serialization;
using Module.Locais.Domain;

namespace Module.Locais.UseCases.AdicionarSala;

public sealed record AdicionarSalaRequest(string SalaNome, int SalaCapacidade, SalaTipo SalaTipo, string? SalaRecursos)
{
    [JsonIgnore]
    public Guid LocalId { get; init; }
}
