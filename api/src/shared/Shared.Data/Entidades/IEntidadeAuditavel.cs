namespace Shared.Data.Entidades;

/// <summary>
/// Campos obrigatórios de auditoria e soft delete de toda entidade principal.
/// Preenchidos automaticamente pelo <c>AuditoriaSaveChangesInterceptor</c>; nunca manualmente.
/// </summary>
public interface IEntidadeAuditavel
{
    Guid Id { get; }
    DateTimeOffset CriadoEm { get; set; }
    string CriadoPor { get; set; }
    DateTimeOffset? AlteradoEm { get; set; }
    string? AlteradoPor { get; set; }
    DateTimeOffset? ExcluidoEm { get; set; }
    string? ExcluidoPor { get; set; }
    bool EstaAtivo { get; set; }
}
