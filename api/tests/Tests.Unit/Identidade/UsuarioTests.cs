using Xunit;
using Module.Identidade.Domain;
using Shared.Contracts.Identidade;
using Shouldly;

namespace Tests.Unit.Identidade;

public sealed class UsuarioTests
{
    [Fact]
    public void Criar_DeveUsarEmailComoLoginGerarGuidV7ERegistrarUsuarioRegistrado()
    {
        var usuario = Usuario.Criar("  Maria Silva ", " maria@exemplo.com ");

        usuario.Id.Version.ShouldBe(7);
        usuario.UsuarioNome.ShouldBe("Maria Silva");
        usuario.Email.ShouldBe("maria@exemplo.com");
        usuario.UserName.ShouldBe("maria@exemplo.com");
        usuario.EstaAtivo.ShouldBeTrue();
        var evento = usuario.Eventos.ShouldHaveSingleItem().ShouldBeOfType<UsuarioRegistrado>();
        evento.UsuarioId.ShouldBe(usuario.Id);
        evento.UsuarioEmail.ShouldBe("maria@exemplo.com");
        evento.UsuarioNome.ShouldBe("Maria Silva");
    }

    [Fact]
    public void RegistrarAcesso_DeveAtualizarUltimoAcessoERegistrarUsuarioAutenticado()
    {
        var usuario = Usuario.Criar("Maria Silva", "maria@exemplo.com");
        usuario.LimparEventos();
        var agora = new DateTimeOffset(2026, 9, 18, 12, 0, 0, TimeSpan.Zero);

        usuario.RegistrarAcesso(agora);

        usuario.UltimoAcessoEm.ShouldBe(agora);
        var evento = usuario.Eventos.ShouldHaveSingleItem().ShouldBeOfType<UsuarioAutenticado>();
        evento.UsuarioId.ShouldBe(usuario.Id);
        evento.UsuarioEmail.ShouldBe("maria@exemplo.com");
    }

    [Theory]
    [InlineData("administrador", PerfisPadrao.Administrador)]
    [InlineData(" ORGANIZADOR ", PerfisPadrao.Organizador)]
    [InlineData("Participante", PerfisPadrao.Participante)]
    public void PerfisValidos_DeveNormalizarParaONomeCanonico(string informado, string esperado)
    {
        var resultado = PerfisValidos.Normalizar([informado, informado]);

        resultado.IsSuccess.ShouldBeTrue();
        resultado.Value.ShouldBe([esperado]);
    }

    [Fact]
    public void PerfisValidos_PerfilDesconhecidoDeveRetornarPerfilInvalido()
    {
        var resultado = PerfisValidos.Normalizar([PerfisPadrao.Administrador, "Root"]);

        resultado.IsFailure.ShouldBeTrue();
        resultado.Error.Code.ShouldBe("Identidade.PerfilInvalido");
    }
}
