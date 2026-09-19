using Module.Eventos.Domain;

namespace Module.Eventos.UseCases.AlterarSituacaoEvento;

public sealed record AlterarSituacaoEventoResponse(Guid Id, EventoSituacao EventoSituacao);
