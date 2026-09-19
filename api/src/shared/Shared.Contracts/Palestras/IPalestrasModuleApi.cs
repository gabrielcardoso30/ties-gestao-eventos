using Shared.Contracts.Integracao;

namespace Shared.Contracts.Palestras;

/// <summary>Contrato síncrono do módulo Palestras (consumido por Eventos: um evento só é publicado com ao menos uma palestra).</summary>
public interface IPalestrasModuleApi
{
    Task<int> ContarPalestrasDoEventoAsync(Guid eventoId, CancellationToken cancellationToken);
}

public sealed record PalestraCriada(Guid PalestraId, Guid EventoId, string PalestraTitulo) : IntegrationEvent;
public sealed record PresencaRegistrada(Guid PalestraId, Guid EventoId, Guid PessoaId) : IntegrationEvent;
public sealed record CertificadoEmitido(Guid CertificadoId, Guid PalestraId, Guid PessoaId, string CertificadoCodigo) : IntegrationEvent;
