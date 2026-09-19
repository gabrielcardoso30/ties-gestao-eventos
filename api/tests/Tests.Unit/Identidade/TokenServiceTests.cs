using Xunit;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Module.Identidade.Domain;
using Module.Identidade.Shared.Seguranca;
using Shared.Contracts.Identidade;
using Shared.WebHost.Security;
using Shouldly;

namespace Tests.Unit.Identidade;

public sealed class TokenServiceTests
{
    private static readonly DateTimeOffset Agora = new(2026, 9, 18, 12, 0, 0, TimeSpan.Zero);
    private const string SigningKey = "chave-de-teste-com-mais-de-32-caracteres-0123456789";

    private static readonly JwtOptions Options = new()
    {
        Issuer = "gestao-eventos-testes",
        Audience = "gestao-eventos-front",
        SigningKey = SigningKey,
        ExpirationMinutes = 90,
    };

    private sealed class RelogioFixo(DateTimeOffset agora) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => agora;
    }

    private static TokenService CriarServico() => new(Microsoft.Extensions.Options.Options.Create(Options), new RelogioFixo(Agora));

    [Fact]
    public void Gerar_DeveEmitirTokenHs256ComIssuerAudienceEExpiracaoDeJwtOptions()
    {
        var usuario = Usuario.Criar("Maria Silva", "maria@exemplo.com");

        var token = CriarServico().Gerar(usuario, [PerfisPadrao.Organizador]);

        var jwt = new JsonWebToken(token.AccessToken);
        jwt.Alg.ShouldBe(SecurityAlgorithms.HmacSha256);
        jwt.Issuer.ShouldBe(Options.Issuer);
        jwt.Audiences.ShouldBe([Options.Audience]);
        token.ExpiraEm.ShouldBe(Agora.AddMinutes(Options.ExpirationMinutes));
        jwt.ValidTo.ShouldBe(token.ExpiraEm.UtcDateTime, tolerance: TimeSpan.FromSeconds(1));
        jwt.ValidFrom.ShouldBe(Agora.UtcDateTime, tolerance: TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Gerar_DeveConterClaimsSubNameEmailJtiEUmaRolePorPerfil()
    {
        var usuario = Usuario.Criar("Maria Silva", "maria@exemplo.com");

        var token = CriarServico().Gerar(usuario, [PerfisPadrao.Administrador, PerfisPadrao.Organizador, PerfisPadrao.Organizador]);

        var jwt = new JsonWebToken(token.AccessToken);
        jwt.Subject.ShouldBe(usuario.Id.ToString());
        jwt.GetClaim(JwtRegisteredClaimNames.Name).Value.ShouldBe("Maria Silva");
        jwt.GetClaim(JwtRegisteredClaimNames.Email).Value.ShouldBe("maria@exemplo.com");
        Guid.TryParse(jwt.GetClaim(JwtRegisteredClaimNames.Jti).Value, out _).ShouldBeTrue();
        jwt.Claims.Where(c => c.Type == "role").Select(c => c.Value).ShouldBe([PerfisPadrao.Administrador, PerfisPadrao.Organizador], ignoreOrder: true);
    }

    [Fact]
    public async Task Gerar_TokenDeveSerValidadoComOsMesmosParametrosDoHost()
    {
        var usuario = Usuario.Criar("Maria Silva", "maria@exemplo.com");
        var token = CriarServico().Gerar(usuario, [PerfisPadrao.Participante]);

        var handler = new JsonWebTokenHandler();
        var resultado = await handler.ValidateTokenAsync(token.AccessToken, new TokenValidationParameters
        {
            ValidIssuer = Options.Issuer,
            ValidAudience = Options.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SigningKey)),
            ValidateLifetime = false, // o relógio do teste é fixo no passado/futuro; a assinatura é o que importa aqui
        });

        resultado.IsValid.ShouldBeTrue(resultado.Exception?.Message);
        resultado.ClaimsIdentity.FindFirst(JwtRegisteredClaimNames.Sub)!.Value.ShouldBe(usuario.Id.ToString());
    }

    [Fact]
    public async Task Gerar_TokenAssinadoComOutraChaveDeveSerRejeitado()
    {
        var usuario = Usuario.Criar("Maria Silva", "maria@exemplo.com");
        var token = CriarServico().Gerar(usuario, []);

        var resultado = await new JsonWebTokenHandler().ValidateTokenAsync(token.AccessToken, new TokenValidationParameters
        {
            ValidIssuer = Options.Issuer,
            ValidAudience = Options.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("outra-chave-qualquer-com-mais-de-32-caracteres-xyz")),
            ValidateLifetime = false,
        });

        resultado.IsValid.ShouldBeFalse();
    }

    [Fact]
    public void Gerar_DeveGerarJtiDiferenteACadaEmissao()
    {
        var usuario = Usuario.Criar("Maria Silva", "maria@exemplo.com");
        var servico = CriarServico();

        var primeiro = new JsonWebToken(servico.Gerar(usuario, []).AccessToken).GetClaim(JwtRegisteredClaimNames.Jti).Value;
        var segundo = new JsonWebToken(servico.Gerar(usuario, []).AccessToken).GetClaim(JwtRegisteredClaimNames.Jti).Value;

        primeiro.ShouldNotBe(segundo);
    }
}
