using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Shared.Contracts.Identidade;

namespace Tests.Integration.Infra;

/// <summary>Emite JWTs válidos para o host de testes sem passar pelo módulo Identidade (isola o que está sendo testado).</summary>
public static class TokenDeTeste
{
    public static string Gerar(string perfil = PerfisPadrao.Administrador, string nome = "Usuário de Teste", Guid? id = null)
    {
        var chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(ApiFactory.JwtSigningKey));
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, (id ?? Guid.CreateVersion7()).ToString()),
            new(JwtRegisteredClaimNames.Name, nome),
            new(JwtRegisteredClaimNames.Email, $"{nome.Replace(' ', '.').ToLowerInvariant()}@teste.local"),
            new(JwtRegisteredClaimNames.Jti, Guid.CreateVersion7().ToString()),
            new("role", perfil),
        };
        var token = new JwtSecurityToken(
            issuer: "gestao-eventos",
            audience: "gestao-eventos",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: new SigningCredentials(chave, SecurityAlgorithms.HmacSha256));
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public static HttpClient ClienteAutenticado(this ApiFactory factory, string perfil = PerfisPadrao.Administrador, string nome = "Usuário de Teste")
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Gerar(perfil, nome));
        return client;
    }
}
