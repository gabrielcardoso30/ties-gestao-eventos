using Microsoft.EntityFrameworkCore;
using Module.Eventos.Domain;
using Module.Eventos.Shared;
using Shared.Contracts.Palestras;
using Shared.Data.Extensions;
using Shared.Http.Endpoints;
using Shared.Http.Results;

namespace Module.Eventos.UseCases.AlterarSituacaoEvento;

/// <summary>Aplica a máquina de estados do agregado. Publicar consulta o módulo Palestras (ao menos uma palestra).</summary>
internal sealed class AlterarSituacaoEventoUseCase(EventosDbContext db, IPalestrasModuleApi palestras) : IUseCase<AlterarSituacaoEventoRequest, AlterarSituacaoEventoResponse>
{
    public async Task<Result<AlterarSituacaoEventoResponse>> HandleAsync(AlterarSituacaoEventoRequest request, CancellationToken cancellationToken)
    {
        var evento = await db.Eventos
            .TagWith("Eventos.AlterarSituacaoEvento.Carregar")
            .FirstOrDefaultAsync(e => e.Id == request.EventoId, cancellationToken);
        if (evento is null)
        {
            return EventosErros.EventoNaoEncontrado;
        }

        var resultado = request.EventoSituacao switch
        {
            EventoSituacao.Publicado => await PublicarAsync(evento, cancellationToken),
            EventoSituacao.EmAndamento => evento.Iniciar(),
            EventoSituacao.Encerrado => evento.Encerrar(),
            EventoSituacao.Cancelado => evento.Cancelar(request.Motivo),
            _ => Result.Failure(EventosErros.TransicaoSituacaoInvalida),
        };
        if (resultado.IsFailure)
        {
            return resultado.Error;
        }

        return await db.ExecuteInTransactionAsync(async ct =>
        {
            await db.SaveChangesAsync(ct);
            return Result.Success(new AlterarSituacaoEventoResponse(evento.Id, evento.EventoSituacao));
        }, cancellationToken);
    }

    private async Task<Result> PublicarAsync(Evento evento, CancellationToken cancellationToken)
    {
        if (evento.EventoSituacao != EventoSituacao.Rascunho)
        {
            return EventosErros.TransicaoSituacaoInvalida;
        }

        var quantidadePalestras = await palestras.ContarPalestrasDoEventoAsync(evento.Id, cancellationToken);
        return evento.Publicar(quantidadePalestras);
    }
}
