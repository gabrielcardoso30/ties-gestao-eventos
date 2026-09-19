using Shared.Contracts.Integracao;

namespace Shared.Contracts.Eventos;

/// <summary>Contrato síncrono do módulo Eventos (consumido por Palestras para validar período, local e inscrição).</summary>
public interface IEventosModuleApi
{
    Task<EventoResumo?> ObterEventoResumoAsync(Guid eventoId, CancellationToken cancellationToken);
    Task<bool> InscricaoConfirmadaExisteAsync(Guid eventoId, Guid pessoaId, CancellationToken cancellationToken);
    Task<TrilhaResumo?> ObterTrilhaResumoAsync(Guid eventoId, Guid trilhaId, CancellationToken cancellationToken);
}

public sealed record EventoResumo(
    Guid Id,
    string EventoNome,
    DateTimeOffset EventoDataInicio,
    DateTimeOffset EventoDataFim,
    string EventoFormato,
    string EventoSituacao,
    Guid? LocalId);

public sealed record TrilhaResumo(Guid Id, Guid EventoId, string TrilhaNome, bool EstaAtivo);

public sealed record EventoPublicado(Guid EventoId, string EventoNome, DateTimeOffset EventoDataInicio) : IntegrationEvent;
public sealed record EventoCancelado(Guid EventoId, string EventoNome, string Motivo) : IntegrationEvent;
public sealed record InscricaoRealizada(Guid InscricaoId, Guid EventoId, Guid PessoaId) : IntegrationEvent;
public sealed record InscricaoCancelada(Guid InscricaoId, Guid EventoId, Guid PessoaId) : IntegrationEvent;
