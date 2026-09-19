namespace Module.Locais.Domain;

/// <summary>Tipos de ambiente de um local. Um local com um único ambiente é representado por uma sala do tipo <see cref="AmbienteUnico"/>.</summary>
public enum SalaTipo
{
    AmbienteUnico,
    Auditorio,
    SalaAula,
    Laboratorio,
    AreaRecreacao,
    Coworking,
    Outro,
}
