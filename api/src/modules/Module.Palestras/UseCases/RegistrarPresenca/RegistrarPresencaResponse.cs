namespace Module.Palestras.UseCases.RegistrarPresenca;

public sealed record RegistrarPresencaResponse(Guid Id, Guid PalestraId, Guid PessoaId, DateTimeOffset PresencaRegistradaEm);
