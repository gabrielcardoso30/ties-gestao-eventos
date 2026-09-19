namespace Module.Auditoria.UseCases.ListarRegistrosAuditoria;

public sealed record ListarRegistrosAuditoriaItemResponse(
    Guid Id,
    string Modulo,
    string EntidadeNome,
    string EntidadeId,
    string Operacao,
    string? UsuarioNome,
    string? TraceId,
    DateTimeOffset OcorridoEm);
