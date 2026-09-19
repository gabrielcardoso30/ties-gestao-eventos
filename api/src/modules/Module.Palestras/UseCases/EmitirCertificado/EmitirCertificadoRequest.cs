using System.Text.Json.Serialization;

namespace Module.Palestras.UseCases.EmitirCertificado;

public sealed record EmitirCertificadoRequest(Guid PessoaId)
{
    [JsonIgnore]
    public Guid PalestraId { get; init; }
}
