namespace Shared.Contracts.Identidade;

/// <summary>Perfis (roles) conhecidos por toda a aplicação.</summary>
public static class PerfisPadrao
{
    public const string Administrador = "Administrador";
    public const string Organizador = "Organizador";
    public const string Participante = "Participante";

    public static readonly IReadOnlyList<string> Todos = [Administrador, Organizador, Participante];
}

/// <summary>Políticas de autorização compartilhadas entre os módulos.</summary>
public static class Politicas
{
    /// <summary>Exige perfil Administrador.</summary>
    public const string Administracao = "Administracao";
    /// <summary>Exige perfil Administrador ou Organizador (gestão de eventos, palestras, locais, pessoas).</summary>
    public const string Gestao = "Gestao";
}
