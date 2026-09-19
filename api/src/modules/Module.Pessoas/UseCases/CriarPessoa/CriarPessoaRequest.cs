namespace Module.Pessoas.UseCases.CriarPessoa;

/// <summary>Dados para cadastro de uma pessoa. O CPF pode vir com máscara; é armazenado apenas com dígitos.</summary>
public sealed record CriarPessoaRequest(
    string PessoaNome,
    string PessoaEmail,
    string? PessoaTelefone,
    string? PessoaDocumento,
    string? PessoaEmpresa,
    string? PessoaCargo,
    string? PessoaMiniBio,
    string? PessoaFotoUrl);
