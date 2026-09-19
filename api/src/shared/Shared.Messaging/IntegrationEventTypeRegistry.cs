using System.Collections.Frozen;
using Shared.Contracts.Integracao;

namespace Shared.Messaging;

/// <summary>Resolve o tipo CLR de um evento a partir do nome gravado no Outbox (todos vivem em Shared.Contracts).</summary>
internal sealed class IntegrationEventTypeRegistry
{
    private readonly FrozenDictionary<string, Type> _tipos = typeof(IIntegrationEvent).Assembly
        .GetTypes()
        .Where(t => t is { IsClass: true, IsAbstract: false } && typeof(IIntegrationEvent).IsAssignableFrom(t))
        .ToFrozenDictionary(t => t.FullName!, t => t);

    public Type? Resolve(string nome) => _tipos.GetValueOrDefault(nome);
}
