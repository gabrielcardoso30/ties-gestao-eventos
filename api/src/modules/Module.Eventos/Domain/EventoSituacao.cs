namespace Module.Eventos.Domain;

/// <summary>
/// Ciclo de vida do evento. Transições válidas:
/// Rascunho → Publicado; Publicado → EmAndamento; Publicado|EmAndamento → Encerrado; Rascunho|Publicado|EmAndamento → Cancelado.
/// </summary>
public enum EventoSituacao
{
    Rascunho,
    Publicado,
    EmAndamento,
    Encerrado,
    Cancelado,
}
