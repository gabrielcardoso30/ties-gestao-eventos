namespace Module.Pessoas.UseCases.ListarPessoas;

public sealed record ListarPessoasItemResponse(Guid Id, string PessoaNome, string PessoaEmail, string? PessoaEmpresa, string? PessoaCargo, bool EstaAtivo);
