namespace Module.Eventos.Domain;

/// <summary>Situação de uma inscrição. O cancelamento não é soft delete: a inscrição permanece com situação <see cref="Cancelada"/>.</summary>
public enum InscricaoSituacao
{
    Confirmada,
    Cancelada,
}
