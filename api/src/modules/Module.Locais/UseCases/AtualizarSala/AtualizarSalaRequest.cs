using System.Text.Json.Serialization;
using Module.Locais.Domain;

namespace Module.Locais.UseCases.AtualizarSala;

public sealed record AtualizarSalaRequest(string SalaNome, int SalaCapacidade, SalaTipo SalaTipo, string? SalaRecursos, bool EstaAtivo)
{
    [JsonIgnore]
    public Guid LocalId { get; init; }

    [JsonIgnore]
    public Guid SalaId { get; init; }
}
