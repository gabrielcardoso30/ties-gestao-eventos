using Module.Eventos.Domain;
using Shouldly;
using Xunit;

namespace Tests.Unit.Eventos;

public sealed class EventoFormatoTests
{
    private static readonly Guid Local = Guid.NewGuid();
    private const string Link = "https://meet.exemplo.com/sala";

    [Theory]
    [InlineData(EventoFormato.Presencial, true, null, true)]
    [InlineData(EventoFormato.Presencial, true, Link, true)]
    [InlineData(EventoFormato.Presencial, false, null, false)]
    [InlineData(EventoFormato.Presencial, false, Link, false)]
    [InlineData(EventoFormato.Remoto, false, Link, true)]
    [InlineData(EventoFormato.Remoto, false, null, false)]
    [InlineData(EventoFormato.Remoto, false, "   ", false)]
    [InlineData(EventoFormato.Remoto, true, Link, false)]
    [InlineData(EventoFormato.Hibrido, true, Link, true)]
    [InlineData(EventoFormato.Hibrido, true, null, false)]
    [InlineData(EventoFormato.Hibrido, false, Link, false)]
    [InlineData(EventoFormato.Hibrido, false, null, false)]
    public void FormatoConsistente_deve_seguir_a_matriz_de_local_e_link(EventoFormato formato, bool comLocal, string? link, bool esperado)
    {
        Evento.FormatoConsistente(formato, comLocal ? Local : null, link).ShouldBe(esperado);
    }

    [Fact]
    public void FormatoConsistente_deve_tratar_Guid_vazio_como_ausencia_de_local()
    {
        Evento.FormatoConsistente(EventoFormato.Presencial, Guid.Empty, null).ShouldBeFalse();
    }

    [Fact]
    public void Criar_presencial_sem_local_deve_falhar_com_FormatoInconsistente()
    {
        var resultado = Evento.Criar("X", null, EventoFabrica.Inicio, EventoFabrica.Fim, EventoFormato.Presencial, null, null, null);

        resultado.IsFailure.ShouldBeTrue();
        resultado.Error.ShouldBe(EventosErros.FormatoInconsistente);
    }

    [Fact]
    public void Criar_remoto_com_local_deve_falhar_com_FormatoInconsistente()
    {
        var resultado = Evento.Criar("X", null, EventoFabrica.Inicio, EventoFabrica.Fim, EventoFormato.Remoto, Local, Link, null);

        resultado.Error.ShouldBe(EventosErros.FormatoInconsistente);
    }

    [Fact]
    public void Criar_hibrido_sem_link_deve_falhar_com_FormatoInconsistente()
    {
        var resultado = Evento.Criar("X", null, EventoFabrica.Inicio, EventoFabrica.Fim, EventoFormato.Hibrido, Local, "", null);

        resultado.Error.ShouldBe(EventosErros.FormatoInconsistente);
    }

    [Fact]
    public void Criar_deve_normalizar_textos_e_guardar_dados()
    {
        var resultado = Evento.Criar("  DevConf  ", "  ", EventoFabrica.Inicio, EventoFabrica.Fim, EventoFormato.Hibrido, Local, $"  {Link} ", 50);

        resultado.IsSuccess.ShouldBeTrue();
        var evento = resultado.Value;
        evento.EventoNome.ShouldBe("DevConf");
        evento.EventoDescricao.ShouldBeNull();
        evento.EventoLinkRemoto.ShouldBe(Link);
        evento.LocalId.ShouldBe(Local);
        evento.EventoFormato.ShouldBe(EventoFormato.Hibrido);
        evento.EventoCapacidadeMaxima.ShouldBe(50);
        evento.EventoDataInicio.ShouldBe(EventoFabrica.Inicio);
        evento.EventoDataFim.ShouldBe(EventoFabrica.Fim);
    }

    [Fact]
    public void Atualizar_com_formato_inconsistente_deve_falhar_e_manter_dados()
    {
        var evento = EventoFabrica.Presencial();

        var resultado = evento.Atualizar("Outro", null, EventoFabrica.Inicio, EventoFabrica.Fim, EventoFormato.Remoto, Local, Link, null);

        resultado.Error.ShouldBe(EventosErros.FormatoInconsistente);
        evento.EventoNome.ShouldBe("DevConf");
        evento.EventoFormato.ShouldBe(EventoFormato.Presencial);
    }

    [Fact]
    public void Atualizar_de_presencial_para_remoto_deve_limpar_local()
    {
        var evento = EventoFabrica.Presencial();

        var resultado = evento.Atualizar("DevConf Online", null, EventoFabrica.Inicio, EventoFabrica.Fim, EventoFormato.Remoto, null, Link, null);

        resultado.IsSuccess.ShouldBeTrue();
        evento.LocalId.ShouldBeNull();
        evento.EventoLinkRemoto.ShouldBe(Link);
        evento.EventoFormato.ShouldBe(EventoFormato.Remoto);
    }
}
