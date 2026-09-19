using Microsoft.EntityFrameworkCore;
using Module.Eventos.Domain;
using Module.Eventos.Shared;
using Shared.Contracts.Locais;
using Shared.Data.Extensions;
using Shared.Http.Endpoints;
using Shared.Http.Results;

namespace Module.Eventos.UseCases.AtualizarEvento;

internal sealed class AtualizarEventoUseCase(EventosDbContext db, ILocaisModuleApi locais) : IUseCase<AtualizarEventoRequest, AtualizarEventoResponse>
{
    public async Task<Result<AtualizarEventoResponse>> HandleAsync(AtualizarEventoRequest request, CancellationToken cancellationToken)
    {
        var evento = await db.Eventos
            .TagWith("Eventos.AtualizarEvento.Carregar")
            .FirstOrDefaultAsync(e => e.Id == request.EventoId, cancellationToken);
        if (evento is null)
        {
            return EventosErros.EventoNaoEncontrado;
        }

        if (!evento.PodeSerAlterado)
        {
            return EventosErros.EventoNaoPodeSerAlterado;
        }

        if (request.LocalId.HasValue)
        {
            var local = await locais.ObterLocalResumoAsync(request.LocalId.Value, cancellationToken);
            if (local is null)
            {
                return EventosErros.LocalNaoEncontrado;
            }
        }

        var resultado = evento.Atualizar(
            request.EventoNome, request.EventoDescricao, request.EventoDataInicio, request.EventoDataFim,
            request.EventoFormato, request.LocalId, request.EventoLinkRemoto, request.EventoCapacidadeMaxima);
        if (resultado.IsFailure)
        {
            return resultado.Error;
        }

        return await db.ExecuteInTransactionAsync(async ct =>
        {
            await db.SaveChangesAsync(ct);
            return Result.Success(new AtualizarEventoResponse(evento.Id, evento.EventoNome, evento.EventoSituacao, evento.AlteradoEm));
        }, cancellationToken);
    }
}
