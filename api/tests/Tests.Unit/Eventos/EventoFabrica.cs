using Module.Eventos.Domain;

namespace Tests.Unit.Eventos;

/// <summary>Fábrica de eventos para os testes: cria em qualquer situação percorrendo transições válidas.</summary>
internal static class EventoFabrica
{
    public static readonly Guid LocalId = Guid.NewGuid();
    public static readonly DateTimeOffset Inicio = new(2026, 10, 1, 9, 0, 0, TimeSpan.Zero);
    public static readonly DateTimeOffset Fim = new(2026, 10, 1, 18, 0, 0, TimeSpan.Zero);

    public static Evento Presencial(int? capacidade = null, Guid? localId = null) =>
        Evento.Criar("DevConf", "Conferência", Inicio, Fim, EventoFormato.Presencial, localId ?? LocalId, null, capacidade).Value;

    public static Evento Remoto(int? capacidade = null) =>
        Evento.Criar("Webinar", null, Inicio, Fim, EventoFormato.Remoto, null, "https://meet.exemplo.com/abc", capacidade).Value;

    public static Evento Hibrido() =>
        Evento.Criar("Meetup", null, Inicio, Fim, EventoFormato.Hibrido, LocalId, "https://meet.exemplo.com/abc", null).Value;

    public static Evento NaSituacao(EventoSituacao situacao, int? capacidade = null)
    {
        var evento = Presencial(capacidade);
        switch (situacao)
        {
            case EventoSituacao.Rascunho:
                break;
            case EventoSituacao.Publicado:
                evento.Publicar(1);
                break;
            case EventoSituacao.EmAndamento:
                evento.Publicar(1);
                evento.Iniciar();
                break;
            case EventoSituacao.Encerrado:
                evento.Publicar(1);
                evento.Encerrar();
                break;
            case EventoSituacao.Cancelado:
                evento.Cancelar("Sem quórum");
                break;
        }

        evento.LimparEventos();
        return evento;
    }
}
