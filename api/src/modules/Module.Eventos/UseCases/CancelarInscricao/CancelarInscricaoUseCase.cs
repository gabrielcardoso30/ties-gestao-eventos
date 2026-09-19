using Microsoft.EntityFrameworkCore;
using Module.Eventos.Domain;
using Module.Eventos.Shared;
using Shared.Data.Extensions;
using Shared.Http.Endpoints;
using Shared.Http.Results;

namespace Module.Eventos.UseCases.CancelarInscricao;

/// <summary>Cancelamento não é soft delete: a inscrição permanece com situação <c>Cancelada</c> e data de cancelamento.</summary>
internal sealed class CancelarInscricaoUseCase(EventosDbContext db, TimeProvider timeProvider) : IUseCase<CancelarInscricaoRequest, CancelarInscricaoResponse>
{
    public async Task<Result<CancelarInscricaoResponse>> HandleAsync(CancelarInscricaoRequest request, CancellationToken cancellationToken)
    {
        var evento = await db.Eventos
            .TagWith("Eventos.CancelarInscricao.Carregar")
            .Include(e => e.Inscricoes.Where(i => i.Id == request.InscricaoId))
            .FirstOrDefaultAsync(e => e.Id == request.EventoId, cancellationToken);
        if (evento is null)
        {
            return EventosErros.EventoNaoEncontrado;
        }

        var resultado = evento.CancelarInscricao(request.InscricaoId, timeProvider.GetUtcNow());
        if (resultado.IsFailure)
        {
            return resultado.Error;
        }

        return await db.ExecuteInTransactionAsync(async ct =>
        {
            await db.SaveChangesAsync(ct);
            return Result.Success(new CancelarInscricaoResponse(resultado.Value.Id));
        }, cancellationToken);
    }
}
