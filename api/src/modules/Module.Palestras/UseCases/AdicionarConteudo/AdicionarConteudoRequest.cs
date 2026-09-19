using System.Text.Json.Serialization;
using Module.Palestras.Domain;

namespace Module.Palestras.UseCases.AdicionarConteudo;

public sealed record AdicionarConteudoRequest(string ConteudoTitulo, ConteudoTipo ConteudoTipo, string ConteudoUrl, string? ConteudoDescricao)
{
    [JsonIgnore]
    public Guid PalestraId { get; init; }
}
