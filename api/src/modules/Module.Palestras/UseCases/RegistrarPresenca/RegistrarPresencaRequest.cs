using System.Text.Json.Serialization;

namespace Module.Palestras.UseCases.RegistrarPresenca;

public sealed record RegistrarPresencaRequest(Guid PessoaId)
{
    [JsonIgnore]
    public Guid PalestraId { get; init; }
}
