using Module.Eventos.Domain;

namespace Module.Eventos.UseCases.InscreverParticipante;

public sealed record InscreverParticipanteResponse(Guid Id, Guid EventoId, Guid PessoaId, InscricaoSituacao InscricaoSituacao, DateTimeOffset InscricaoRealizadaEm);
