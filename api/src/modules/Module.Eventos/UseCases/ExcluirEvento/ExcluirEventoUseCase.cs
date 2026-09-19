using Microsoft.EntityFrameworkCore;
using Module.Eventos.Domain;
using Module.Eventos.Shared;
using Shared.Data.Extensions;
using Shared.Http.Endpoints;
using Shared.Http.Results;

namespace Module.Eventos.UseCases.ExcluirEvento;

/// <summary>Exclusão lógica do evento e de suas inscrições (interceptor converte Remove em soft delete).</summary>
internal sealed class ExcluirEventoUseCase(EventosDbContext db) : IUseCase<ExcluirEventoRequest, ExcluirEventoResponse>
{
    public async Task<Result<ExcluirEventoResponse>> HandleAsync(ExcluirEventoRequest request, CancellationToken cancellationToken)
    {
        var evento = await db.Eventos
            .TagWith("Eventos.ExcluirEvento.Carregar")
            .Include(e => e.Inscricoes)
            .FirstOrDefaultAsync(e => e.Id == request.EventoId, cancellationToken);
        if (evento is null)
        {
            return EventosErros.EventoNaoEncontrado;
        }

        var resultado = evento.MarcarExcluido();
        if (resultado.IsFailure)
        {
            return resultado.Error;
        }

        return await db.ExecuteInTransactionAsync(async ct =>
        {
            db.Inscricoes.RemoveRange(evento.Inscricoes);
            db.Eventos.Remove(evento);
            await db.SaveChangesAsync(ct);
            return Result.Success(new ExcluirEventoResponse(evento.Id));
        }, cancellationToken);
    }
}
