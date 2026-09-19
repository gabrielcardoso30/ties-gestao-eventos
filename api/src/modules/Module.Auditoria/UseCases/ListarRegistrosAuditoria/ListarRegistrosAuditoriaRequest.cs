namespace Module.Auditoria.UseCases.ListarRegistrosAuditoria;

/// <summary>Filtros de consulta da trilha de auditoria (query string). Todos opcionais; combinados com AND.</summary>
public sealed record ListarRegistrosAuditoriaRequest(
    string? Modulo,
    string? EntidadeNome,
    string? EntidadeId,
    Guid? UsuarioId,
    string? Operacao,
    DateTimeOffset? OcorridoDe,
    DateTimeOffset? OcorridoAte,
    int Pagina = 1,
    int TamanhoPagina = 20);
