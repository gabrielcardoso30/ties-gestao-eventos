using Module.Eventos.Domain;
using Module.Eventos.Shared;
using Shared.Contracts.Locais;
using Shared.Data.Extensions;
using Shared.Http.Endpoints;
using Shared.Http.Results;

namespace Module.Eventos.UseCases.CriarEvento;

internal sealed class CriarEventoUseCase(EventosDbContext db, ILocaisModuleApi locais) : IUseCase<CriarEventoRequest, CriarEventoResponse>
{
    public async Task<Result<CriarEventoResponse>> HandleAsync(CriarEventoRequest request, CancellationToken cancellationToken)
    {
        if (request.LocalId.HasValue)
        {
            var local = await locais.ObterLocalResumoAsync(request.LocalId.Value, cancellationToken);
            if (local is null)
            {
                return EventosErros.LocalNaoEncontrado;
            }
        }

        var resultado = Evento.Criar(
            request.EventoNome, request.EventoDescricao, request.EventoDataInicio, request.EventoDataFim,
            request.EventoFormato, request.LocalId, request.EventoLinkRemoto, request.EventoCapacidadeMaxima);
        if (resultado.IsFailure)
        {
            return resultado.Error;
        }

        var evento = resultado.Value;
        var trilhas = request.Trilhas is { Count: > 0 }
            ? request.Trilhas
            : [new CriarEventoTrilhaRequest("Trilha única", null, "#2563EB")];
        foreach (var item in trilhas)
        {
            var trilhaResultado = evento.AdicionarTrilha(item.TrilhaNome, item.TrilhaDescricao, item.TrilhaCor);
            if (trilhaResultado.IsFailure) return trilhaResultado.Error;
        }
        return await db.ExecuteInTransactionAsync(async ct =>
        {
            db.Eventos.Add(evento);
            await db.SaveChangesAsync(ct);
            return Result.Success(new CriarEventoResponse(evento.Id, evento.EventoNome, evento.EventoSituacao));
        }, cancellationToken);
    }
}
