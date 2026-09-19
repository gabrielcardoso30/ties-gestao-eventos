namespace Module.Pessoas.UseCases.ObterPessoa;

public sealed record ObterPessoaResponse(
    Guid Id,
    string PessoaNome,
    string PessoaEmail,
    string? PessoaTelefone,
    string? PessoaDocumento,
    string? PessoaEmpresa,
    string? PessoaCargo,
    string? PessoaMiniBio,
    string? PessoaFotoUrl,
    bool EstaAtivo,
    DateTimeOffset CriadoEm,
    DateTimeOffset? AlteradoEm);
