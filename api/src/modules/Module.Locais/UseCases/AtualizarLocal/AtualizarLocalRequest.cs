using System.Text.Json.Serialization;

namespace Module.Locais.UseCases.AtualizarLocal;

public sealed record AtualizarLocalRequest(
    string LocalNome,
    string? LocalDescricao,
    string? EnderecoLogradouro,
    string? EnderecoNumero,
    string? EnderecoBairro,
    string EnderecoCidade,
    string EnderecoUf,
    string? EnderecoCep)
{
    /// <summary>Preenchido pela rota; não faz parte do corpo.</summary>
    [JsonIgnore]
    public Guid LocalId { get; init; }
}
