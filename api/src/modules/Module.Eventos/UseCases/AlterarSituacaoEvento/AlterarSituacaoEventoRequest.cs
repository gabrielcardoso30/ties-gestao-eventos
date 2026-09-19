using System.Text.Json.Serialization;
using Module.Eventos.Domain;

namespace Module.Eventos.UseCases.AlterarSituacaoEvento;

/// <summary>Nova situação desejada. <c>Motivo</c> é obrigatório apenas para <c>Cancelado</c>.</summary>
public sealed record AlterarSituacaoEventoRequest(EventoSituacao EventoSituacao, string? Motivo)
{
    /// <summary>Preenchido pela rota; não faz parte do corpo.</summary>
    [JsonIgnore]
    public Guid EventoId { get; init; }
}
