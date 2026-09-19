namespace Module.Pessoas.UseCases.ListarPessoas;

/// <summary>Filtros de listagem (query string). <c>Busca</c> aplica sobre nome, e-mail e empresa.</summary>
public sealed record ListarPessoasRequest(string? Busca, bool? EstaAtivo, int Pagina = 1, int TamanhoPagina = 20, string? OrdenarPor = null, global::Shared.Contracts.Common.OrdenacaoDirecao Direcao = global::Shared.Contracts.Common.OrdenacaoDirecao.Asc);
