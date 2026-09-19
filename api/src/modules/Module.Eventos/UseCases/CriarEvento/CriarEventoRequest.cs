using Module.Eventos.Domain;

namespace Module.Eventos.UseCases.CriarEvento;

/// <summary>Dados para criação de um evento. O evento nasce em <c>Rascunho</c>.</summary>
public sealed record CriarEventoRequest(
    string EventoNome,
    string? EventoDescricao,
    DateTimeOffset EventoDataInicio,
    DateTimeOffset EventoDataFim,
    EventoFormato EventoFormato,
    Guid? LocalId,
    string? EventoLinkRemoto,
    int? EventoCapacidadeMaxima,
    IReadOnlyList<CriarEventoTrilhaRequest>? Trilhas = null);

public sealed record CriarEventoTrilhaRequest(string TrilhaNome, string? TrilhaDescricao, string? TrilhaCor);
