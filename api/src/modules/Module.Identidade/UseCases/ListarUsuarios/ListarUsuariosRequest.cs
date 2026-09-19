namespace Module.Identidade.UseCases.ListarUsuarios;

/// <summary>Filtros de listagem (query string). <c>Busca</c> aplica sobre nome e e-mail.</summary>
public sealed record ListarUsuariosRequest(string? Busca, bool? EstaAtivo, int Pagina = 1, int TamanhoPagina = 20, string? OrdenarPor = null, global::Shared.Contracts.Common.OrdenacaoDirecao Direcao = global::Shared.Contracts.Common.OrdenacaoDirecao.Asc);
