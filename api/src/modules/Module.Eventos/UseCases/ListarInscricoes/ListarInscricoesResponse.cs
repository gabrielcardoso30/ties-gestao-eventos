using Module.Eventos.Domain;

namespace Module.Eventos.UseCases.ListarInscricoes;

public sealed record ListarInscricoesItemResponse(
    Guid Id,
    Guid PessoaId,
    string PessoaNome,
    string PessoaEmail,
    InscricaoSituacao InscricaoSituacao,
    DateTimeOffset InscricaoRealizadaEm);
