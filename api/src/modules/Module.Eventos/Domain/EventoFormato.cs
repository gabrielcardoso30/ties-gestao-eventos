namespace Module.Eventos.Domain;

/// <summary>Formato de realização do evento. Define quais dados de local/link são obrigatórios.</summary>
public enum EventoFormato
{
    /// <summary>Exige <c>LocalId</c>.</summary>
    Presencial,

    /// <summary>Exige <c>EventoLinkRemoto</c> e não admite <c>LocalId</c>.</summary>
    Remoto,

    /// <summary>Exige <c>LocalId</c> e <c>EventoLinkRemoto</c>.</summary>
    Hibrido,
}
