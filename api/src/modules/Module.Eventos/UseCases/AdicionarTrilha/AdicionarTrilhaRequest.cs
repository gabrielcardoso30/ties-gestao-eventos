using System.Text.Json.Serialization;
namespace Module.Eventos.UseCases.AdicionarTrilha;
public sealed record AdicionarTrilhaRequest(string TrilhaNome, string? TrilhaDescricao, string? TrilhaCor)
{
    [JsonIgnore] public Guid EventoId { get; init; }
}
