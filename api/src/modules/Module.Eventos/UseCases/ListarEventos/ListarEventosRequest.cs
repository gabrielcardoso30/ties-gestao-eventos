using Module.Eventos.Domain;

namespace Module.Eventos.UseCases.ListarEventos;

/// <summary>Filtros de listagem (query string). <c>Busca</c> aplica sobre nome e descrição; datas filtram <c>EventoDataInicio</c>.</summary>
public sealed record ListarEventosRequest(
    string? Busca,
    EventoSituacao? EventoSituacao,
    EventoFormato? EventoFormato,
    DateTimeOffset? DataInicioDe,
    DateTimeOffset? DataInicioAte,
    int Pagina = 1,
    int TamanhoPagina = 20);
