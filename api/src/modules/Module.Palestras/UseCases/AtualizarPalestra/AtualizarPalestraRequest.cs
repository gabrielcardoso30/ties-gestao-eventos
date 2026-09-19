using System.Text.Json.Serialization;

namespace Module.Palestras.UseCases.AtualizarPalestra;

public sealed record AtualizarPalestraRequest(
    Guid? SalaId,
    string PalestraTitulo,
    string? PalestraDescricao,
    DateTimeOffset PalestraInicio,
    DateTimeOffset PalestraFim)
{
    /// <summary>Preenchido pela rota; não faz parte do corpo.</summary>
    [JsonIgnore]
    public Guid PalestraId { get; init; }
}
