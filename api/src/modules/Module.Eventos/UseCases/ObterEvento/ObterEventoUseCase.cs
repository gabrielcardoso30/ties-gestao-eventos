using Microsoft.EntityFrameworkCore;
using Module.Eventos.Domain;
using Module.Eventos.Shared;
using Shared.Contracts.Locais;
using Shared.Http.Endpoints;
using Shared.Http.Results;

namespace Module.Eventos.UseCases.ObterEvento;

internal sealed class ObterEventoUseCase(EventosDbContext db, ILocaisModuleApi locais) : IUseCase<ObterEventoRequest, ObterEventoResponse>
{
    public async Task<Result<ObterEventoResponse>> HandleAsync(ObterEventoRequest request, CancellationToken cancellationToken)
    {
        var evento = await db.Eventos
            .TagWith("Eventos.ObterEvento")
            .AsNoTracking()
            .Where(e => e.Id == request.EventoId)
            .Select(e => new ObterEventoResponse(
                e.Id, e.EventoNome, e.EventoDescricao, e.EventoDataInicio, e.EventoDataFim, e.EventoFormato, e.EventoSituacao,
                e.LocalId, null, e.EventoLinkRemoto, e.EventoCapacidadeMaxima,
                e.Inscricoes.Count(i => i.InscricaoSituacao == InscricaoSituacao.Confirmada),
                e.EventoCancelamentoMotivo, e.CriadoEm, e.AlteradoEm))
            .FirstOrDefaultAsync(cancellationToken);
        if (evento is null)
        {
            return EventosErros.EventoNaoEncontrado;
        }

        if (evento.LocalId is null)
        {
            return evento;
        }

        var local = await locais.ObterLocalResumoAsync(evento.LocalId.Value, cancellationToken);
        return evento with { LocalNome = local?.LocalNome };
    }
}
