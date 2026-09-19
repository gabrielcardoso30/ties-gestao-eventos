using Module.Eventos.Domain;
using Shouldly;

namespace Tests.Unit.Eventos;

public sealed class EventoTrilhaTests
{
    private static Evento NovoEvento() => Evento.Criar("Evento", null, DateTimeOffset.UtcNow.AddDays(1), DateTimeOffset.UtcNow.AddDays(2), EventoFormato.Remoto, null, "https://evento.test", null).Value;

    [Fact]
    public void Deve_adicionar_atualizar_e_normalizar_trilha()
    {
        var evento = NovoEvento();
        var criada = evento.AdicionarTrilha("  Backend  ", "  APIs e dados  ", "#aabbcc");
        criada.IsSuccess.ShouldBeTrue();
        criada.Value.TrilhaNome.ShouldBe("Backend");
        criada.Value.TrilhaCor.ShouldBe("#AABBCC");
        evento.AtualizarTrilha(criada.Value.Id, "Arquitetura", null, "#112233").IsSuccess.ShouldBeTrue();
        criada.Value.TrilhaNome.ShouldBe("Arquitetura");
    }

    [Fact]
    public void Nomes_repetidos_devem_ser_rejeitados_sem_diferenciar_maiusculas()
    {
        var evento = NovoEvento(); evento.AdicionarTrilha("Cloud", null, null);
        evento.AdicionarTrilha(" cloud ", null, null).Error.ShouldBe(EventosErros.TrilhaNomeDuplicado);
    }

    [Fact]
    public void Evento_deve_manter_ao_menos_uma_trilha()
    {
        var evento = NovoEvento(); var primeira = evento.AdicionarTrilha("Única", null, null).Value;
        evento.RemoverTrilha(primeira.Id).Error.ShouldBe(EventosErros.EventoPrecisaDeUmaTrilha);
        var segunda = evento.AdicionarTrilha("Outra", null, null).Value;
        evento.RemoverTrilha(segunda.Id).IsSuccess.ShouldBeTrue();
    }
}
