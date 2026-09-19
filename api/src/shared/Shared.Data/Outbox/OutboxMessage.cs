namespace Shared.Data.Outbox;

/// <summary>
/// Mensagem do padrão Outbox. Cada módulo possui sua própria tabela <c>OutboxMessages</c> no seu schema,
/// gravada na MESMA transação da alteração de negócio. Um processador em segundo plano publica as mensagens.
/// </summary>
public sealed class OutboxMessage
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
    public required string Type { get; init; }
    public required string Payload { get; init; }
    public DateTimeOffset OccurredOn { get; init; }
    public DateTimeOffset? ProcessedOn { get; set; }
    public int Attempts { get; set; }
    public string? Error { get; set; }
    public DateTimeOffset? LockedUntil { get; set; }
    public string? TraceParent { get; init; }
}
