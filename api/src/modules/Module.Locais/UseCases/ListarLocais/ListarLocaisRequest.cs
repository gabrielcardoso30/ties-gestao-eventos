namespace Module.Locais.UseCases.ListarLocais;

/// <summary>Filtros de listagem (query string). <c>Busca</c> aplica sobre nome e cidade.</summary>
public sealed record ListarLocaisRequest(string? Busca, string? EnderecoUf, bool? EstaAtivo, int Pagina = 1, int TamanhoPagina = 20);
