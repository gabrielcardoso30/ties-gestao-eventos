using Module.Eventos.Domain;

namespace Module.Eventos.UseCases.ObterEvento;

public sealed record ObterEventoResponse(
    Guid Id,
    string EventoNome,
    string? EventoDescricao,
    DateTimeOffset EventoDataInicio,
    DateTimeOffset EventoDataFim,
    EventoFormato EventoFormato,
    EventoSituacao EventoSituacao,
    Guid? LocalId,
    string? LocalNome,
    string? EventoLinkRemoto,
    int? EventoCapacidadeMaxima,
    int InscricoesConfirmadas,
    string? EventoCancelamentoMotivo,
    DateTimeOffset CriadoEm,
    DateTimeOffset? AlteradoEm);
