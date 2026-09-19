namespace Module.Palestras.UseCases.ListarPresencas;

public sealed record ListarPresencasItemResponse(Guid Id, Guid PessoaId, string PessoaNome, DateTimeOffset PresencaRegistradaEm, bool CertificadoEmitido);
