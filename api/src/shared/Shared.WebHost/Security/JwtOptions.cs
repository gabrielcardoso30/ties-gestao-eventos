namespace Shared.WebHost.Security;

/// <summary>Configuração do JWT. A chave NUNCA vai para o appsettings de produção: use variável de ambiente/Key Vault.</summary>
public sealed class JwtOptions
{
    public const string Secao = "Jwt";
    public string Issuer { get; set; } = "gestao-eventos";
    public string Audience { get; set; } = "gestao-eventos";
    public string SigningKey { get; set; } = string.Empty;
    public int ExpirationMinutes { get; set; } = 480;
}
