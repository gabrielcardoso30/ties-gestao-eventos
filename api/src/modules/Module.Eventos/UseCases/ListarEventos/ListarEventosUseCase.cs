using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Module.Eventos.Domain;
using Module.Eventos.Shared;
using Shared.Contracts.Common;
using Shared.Data.Extensions;
using Shared.Http.Endpoints;
using Shared.Http.Results;

namespace Module.Eventos.UseCases.ListarEventos;

internal sealed class ListarEventosUseCase(EventosDbContext db, ILogger<ListarEventosUseCase> logger) : IUseCase<ListarEventosRequest, PagedResult<ListarEventosItemResponse>>
{
    public async Task<Result<PagedResult<ListarEventosItemResponse>>> HandleAsync(ListarEventosRequest request, CancellationToken cancellationToken)
    {
        var query = db.Eventos.TagWith("Eventos.ListarEventos").AsNoTracking();
        logger.LogDebug("Montando listagem de eventos: busca={HasSearch}, situação={HasStatus}, formato={HasFormat}, inícioDe={HasStartFrom}, inícioAté={HasStartTo}",
            !string.IsNullOrWhiteSpace(request.Busca), request.EventoSituacao.HasValue, request.EventoFormato.HasValue, request.DataInicioDe.HasValue, request.DataInicioAte.HasValue);

        if (!string.IsNullOrWhiteSpace(request.Busca))
        {
            var busca = $"%{request.Busca.Trim()}%";
            query = query.Where(e => EF.Functions.ILike(e.EventoNome, busca) || (e.EventoDescricao != null && EF.Functions.ILike(e.EventoDescricao, busca)));
            logger.LogDebug("Filtro textual aplicado à listagem de eventos sem registrar seu conteúdo");
        }

        if (request.EventoSituacao.HasValue)
        {
            query = query.Where(e => e.EventoSituacao == request.EventoSituacao.Value);
            logger.LogDebug("Filtro de situação {EventoSituacao} aplicado à listagem de eventos", request.EventoSituacao);
        }

        if (request.EventoFormato.HasValue)
        {
            query = query.Where(e => e.EventoFormato == request.EventoFormato.Value);
            logger.LogDebug("Filtro de formato {EventoFormato} aplicado à listagem de eventos", request.EventoFormato);
        }

        if (request.DataInicioDe.HasValue)
        {
            query = query.Where(e => e.EventoDataInicio >= request.DataInicioDe.Value);
            logger.LogDebug("Limite inicial aplicado à listagem de eventos");
        }

        if (request.DataInicioAte.HasValue)
        {
            query = query.Where(e => e.EventoDataInicio <= request.DataInicioAte.Value);
            logger.LogDebug("Limite final aplicado à listagem de eventos");
        }

        var descendente = request.Direcao == OrdenacaoDirecao.Desc;
        logger.LogDebug("Ordenando eventos por {SortField} em direção {SortDirection}", request.OrdenarPor ?? "eventoDataInicio", request.Direcao);
        var ordenada = request.OrdenarPor?.ToLowerInvariant() switch
        {
            "eventonome" => descendente ? query.OrderByDescending(x => x.EventoNome).ThenByDescending(x => x.Id) : query.OrderBy(x => x.EventoNome).ThenBy(x => x.Id),
            "eventoformato" => descendente ? query.OrderByDescending(x => x.EventoFormato).ThenByDescending(x => x.Id) : query.OrderBy(x => x.EventoFormato).ThenBy(x => x.Id),
            "eventosituacao" => descendente ? query.OrderByDescending(x => x.EventoSituacao).ThenByDescending(x => x.Id) : query.OrderBy(x => x.EventoSituacao).ThenBy(x => x.Id),
            "inscricoesconfirmadas" => descendente ? query.OrderByDescending(x => x.Inscricoes.Count(i => i.InscricaoSituacao == InscricaoSituacao.Confirmada)).ThenByDescending(x => x.Id) : query.OrderBy(x => x.Inscricoes.Count(i => i.InscricaoSituacao == InscricaoSituacao.Confirmada)).ThenBy(x => x.Id),
            _ => descendente ? query.OrderByDescending(x => x.EventoDataInicio).ThenByDescending(x => x.Id) : query.OrderBy(x => x.EventoDataInicio).ThenBy(x => x.Id)
        };
        var pagina = await ordenada
            .Select(e => new ListarEventosItemResponse(e.Id, e.EventoNome, e.EventoDataInicio, e.EventoDataFim, e.EventoFormato, e.EventoSituacao, e.LocalId, e.Inscricoes.Count(i => i.InscricaoSituacao == InscricaoSituacao.Confirmada)))
            .ToPagedResultAsync(new PagedRequest(request.Pagina, request.TamanhoPagina), cancellationToken);

        logger.LogInformation("Listagem de eventos retornou {ReturnedCount} de {TotalCount} registro(s)", pagina.Itens.Count, pagina.Total);

        return pagina;
    }
}
