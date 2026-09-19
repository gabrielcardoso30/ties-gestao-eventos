namespace Module.Auditoria.UseCases.ObterRegistroAuditoria;

/// <summary><c>DadosAnteriores</c> e <c>DadosNovos</c> são strings JSON (conteúdo do jsonb), para o cliente renderizar o diff.</summary>
public sealed record ObterRegistroAuditoriaResponse(
    Guid Id,
    string Modulo,
    string EntidadeNome,
    string EntidadeId,
    string Operacao,
    string? DadosAnteriores,
    string? DadosNovos,
    Guid? UsuarioId,
    string? UsuarioNome,
    string? TraceId,
    DateTimeOffset OcorridoEm,
    DateTimeOffset RegistradoEm);
