using System.Text.Json.Serialization;

namespace Module.Pessoas.UseCases.AtualizarPessoa;

/// <summary>Atualização completa da pessoa (PUT): todos os campos são substituídos, inclusive <c>EstaAtivo</c>.</summary>
public sealed record AtualizarPessoaRequest(
    string PessoaNome,
    string PessoaEmail,
    string? PessoaTelefone,
    string? PessoaDocumento,
    string? PessoaEmpresa,
    string? PessoaCargo,
    string? PessoaMiniBio,
    string? PessoaFotoUrl,
    bool EstaAtivo = true)
{
    /// <summary>Preenchido pela rota; não faz parte do corpo.</summary>
    [JsonIgnore]
    public Guid PessoaId { get; init; }
}
