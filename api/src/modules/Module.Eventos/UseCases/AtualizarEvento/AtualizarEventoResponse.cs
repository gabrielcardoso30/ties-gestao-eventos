using Module.Eventos.Domain;

namespace Module.Eventos.UseCases.AtualizarEvento;

public sealed record AtualizarEventoResponse(Guid Id, string EventoNome, EventoSituacao EventoSituacao, DateTimeOffset? AlteradoEm);
