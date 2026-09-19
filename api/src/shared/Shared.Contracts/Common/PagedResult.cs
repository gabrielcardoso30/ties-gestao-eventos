namespace Shared.Contracts.Common;

public enum OrdenacaoDirecao { Asc, Desc }

/// <summary>Resultado paginado padrão de todas as listagens da API.</summary>
public sealed record PagedResult<T>(IReadOnlyList<T> Itens, int Pagina, int TamanhoPagina, long Total)
{
    public int TotalPaginas => TamanhoPagina == 0 ? 0 : (int)Math.Ceiling(Total / (double)TamanhoPagina);
}

/// <summary>Parâmetros de paginação aceitos por todas as listagens (query string).</summary>
public sealed record PagedRequest(int Pagina = 1, int TamanhoPagina = 20)
{
    public const int TamanhoMaximo = 100;
    public int PaginaNormalizada => Pagina < 1 ? 1 : Pagina;
    public int TamanhoNormalizado => TamanhoPagina < 1 ? 20 : Math.Min(TamanhoPagina, TamanhoMaximo);
}
