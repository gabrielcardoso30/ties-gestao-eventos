using System.Text.Json.Serialization;

namespace Module.Eventos.UseCases.InscreverParticipante;

public sealed record InscreverParticipanteRequest(Guid PessoaId)
{
    /// <summary>Preenchido pela rota; não faz parte do corpo.</summary>
    [JsonIgnore]
    public Guid EventoId { get; init; }
}
