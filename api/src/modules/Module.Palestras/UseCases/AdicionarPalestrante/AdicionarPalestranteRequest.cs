using System.Text.Json.Serialization;
using Module.Palestras.Domain;

namespace Module.Palestras.UseCases.AdicionarPalestrante;

public sealed record AdicionarPalestranteRequest(Guid PessoaId, PalestrantePapel PalestrantePapel)
{
    [JsonIgnore]
    public Guid PalestraId { get; init; }
}
