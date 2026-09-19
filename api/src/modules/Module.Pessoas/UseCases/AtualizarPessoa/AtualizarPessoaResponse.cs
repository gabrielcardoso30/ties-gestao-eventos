namespace Module.Pessoas.UseCases.AtualizarPessoa;

public sealed record AtualizarPessoaResponse(Guid Id, string PessoaNome, string PessoaEmail, bool EstaAtivo, DateTimeOffset? AlteradoEm);
