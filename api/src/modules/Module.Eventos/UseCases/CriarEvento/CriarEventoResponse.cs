using Module.Eventos.Domain;

namespace Module.Eventos.UseCases.CriarEvento;

public sealed record CriarEventoResponse(Guid Id, string EventoNome, EventoSituacao EventoSituacao);
