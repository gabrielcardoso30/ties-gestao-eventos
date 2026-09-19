namespace Shared.Messaging;

public sealed class OutboxOptions
{
    public const string Secao = "Outbox";
    public bool Enabled { get; set; } = true;
    public int PollingIntervalMs { get; set; } = 2000;
    public int BatchSize { get; set; } = 50;
    public int LockSeconds { get; set; } = 60;
    public int MaxAttempts { get; set; } = 10;
}
