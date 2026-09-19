using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Module.Identidade.Domain;
using Shared.WebHost.Security;

namespace Module.Identidade.Shared.Seguranca;

/// <summary>Token de acesso emitido no login.</summary>
public sealed record TokenGerado(string AccessToken, DateTimeOffset ExpiraEm);

/// <summary>Emite o JWT de acesso do usuário autenticado.</summary>
public interface ITokenService
{
    TokenGerado Gerar(Usuario usuario, IReadOnlyCollection<string> perfis);
}

/// <summary>
/// Emissor de JWT compatível com a validação do host (<c>JwtBearer</c> em Shared.WebHost): HS256 com <c>Jwt:SigningKey</c>,
/// issuer/audience de <see cref="JwtOptions"/> e claims curtas (<c>sub</c>, <c>name</c>, <c>email</c>, <c>role</c>, <c>jti</c>),
/// que o host mapeia para os ClaimTypes do .NET (<c>MapInboundClaims = true</c>).
/// </summary>
internal sealed class TokenService(IOptions<JwtOptions> options, TimeProvider timeProvider) : ITokenService
{
    private static readonly JsonWebTokenHandler Handler = new() { SetDefaultTimesOnTokenCreation = false };

    public TokenGerado Gerar(Usuario usuario, IReadOnlyCollection<string> perfis)
    {
        var jwt = options.Value;
        var agora = timeProvider.GetUtcNow();
        var expiraEm = agora.AddMinutes(jwt.ExpirationMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new(JwtRegisteredClaimNames.Name, usuario.UsuarioNome),
            new(JwtRegisteredClaimNames.Email, usuario.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.CreateVersion7().ToString()),
        };
        claims.AddRange(perfis.Distinct(StringComparer.Ordinal).Select(perfil => new Claim("role", perfil)));

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = jwt.Issuer,
            Audience = jwt.Audience,
            Subject = new ClaimsIdentity(claims),
            IssuedAt = agora.UtcDateTime,
            NotBefore = agora.UtcDateTime,
            Expires = expiraEm.UtcDateTime,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)), SecurityAlgorithms.HmacSha256),
        };

        return new TokenGerado(Handler.CreateToken(descriptor), expiraEm);
    }
}
