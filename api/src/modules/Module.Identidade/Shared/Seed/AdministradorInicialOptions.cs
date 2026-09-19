namespace Module.Identidade.Shared.Seed;

/// <summary>Administrador criado na subida (seção <c>Identidade:AdministradorInicial</c>). A senha só deve vir de variável de ambiente/segredo.</summary>
public sealed class AdministradorInicialOptions
{
    public const string Secao = "Identidade:AdministradorInicial";
    public string Email { get; set; } = string.Empty;
    public string Nome { get; set; } = "Administrador";
    public string Senha { get; set; } = string.Empty;
}
