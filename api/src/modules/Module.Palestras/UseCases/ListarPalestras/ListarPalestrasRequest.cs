namespace Module.Palestras.UseCases.ListarPalestras;

/// <summary>Filtros de listagem (query string). <c>Busca</c> aplica sobre o título.</summary>
public sealed record ListarPalestrasRequest(Guid? EventoId, string? Busca, int Pagina = 1, int TamanhoPagina = 20, string? OrdenarPor = null, global::Shared.Contracts.Common.OrdenacaoDirecao Direcao = global::Shared.Contracts.Common.OrdenacaoDirecao.Asc);
