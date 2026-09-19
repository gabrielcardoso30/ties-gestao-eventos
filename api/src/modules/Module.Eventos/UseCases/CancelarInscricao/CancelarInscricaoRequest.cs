namespace Module.Eventos.UseCases.CancelarInscricao;

public sealed record CancelarInscricaoRequest(Guid EventoId, Guid InscricaoId);
