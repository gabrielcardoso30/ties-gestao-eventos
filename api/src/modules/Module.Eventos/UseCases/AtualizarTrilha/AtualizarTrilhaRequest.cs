using System.Text.Json.Serialization;
namespace Module.Eventos.UseCases.AtualizarTrilha;
public sealed record AtualizarTrilhaRequest(string TrilhaNome, string? TrilhaDescricao, string? TrilhaCor, bool EstaAtivo)
{
    [JsonIgnore] public Guid EventoId { get; init; }
    [JsonIgnore] public Guid TrilhaId { get; init; }
}
