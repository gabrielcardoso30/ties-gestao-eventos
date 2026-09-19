using Shared.Contracts.Integracao;

namespace Shared.Data.Entidades;

/// <summary>
/// Base de todas as entidades principais: PK Guid v7 (ordenável, amigável a índices B-tree),
/// campos de auditoria, soft delete e emissão de eventos de integração.
/// </summary>
public abstract class EntidadeBase : IEntidadeAuditavel, IEmissorDeEventos
{
    private readonly List<IIntegrationEvent> _eventos = [];

    public Guid Id { get; protected set; } = Guid.CreateVersion7();
    public DateTimeOffset CriadoEm { get; set; }
    public string CriadoPor { get; set; } = string.Empty;
    public DateTimeOffset? AlteradoEm { get; set; }
    public string? AlteradoPor { get; set; }
    public DateTimeOffset? ExcluidoEm { get; set; }
    public string? ExcluidoPor { get; set; }
    public bool EstaAtivo { get; set; } = true;

    public IReadOnlyCollection<IIntegrationEvent> Eventos => _eventos.AsReadOnly();

    protected void RegistrarEvento(IIntegrationEvent integrationEvent) => _eventos.Add(integrationEvent);

    public void LimparEventos() => _eventos.Clear();

    public void Ativar() => EstaAtivo = true;

    public void Desativar() => EstaAtivo = false;
}
