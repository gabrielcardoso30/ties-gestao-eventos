using System.Text.Json.Serialization;
using Module.Eventos.Domain;

namespace Module.Eventos.UseCases.AtualizarEvento;

public sealed record AtualizarEventoRequest(
    string EventoNome,
    string? EventoDescricao,
    DateTimeOffset EventoDataInicio,
    DateTimeOffset EventoDataFim,
    EventoFormato EventoFormato,
    Guid? LocalId,
    string? EventoLinkRemoto,
    int? EventoCapacidadeMaxima)
{
    /// <summary>Preenchido pela rota; não faz parte do corpo.</summary>
    [JsonIgnore]
    public Guid EventoId { get; init; }
}
