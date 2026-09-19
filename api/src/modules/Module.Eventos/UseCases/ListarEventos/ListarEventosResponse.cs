using Module.Eventos.Domain;

namespace Module.Eventos.UseCases.ListarEventos;

public sealed record ListarEventosItemResponse(
    Guid Id,
    string EventoNome,
    DateTimeOffset EventoDataInicio,
    DateTimeOffset EventoDataFim,
    EventoFormato EventoFormato,
    EventoSituacao EventoSituacao,
    Guid? LocalId,
    int InscricoesConfirmadas);
