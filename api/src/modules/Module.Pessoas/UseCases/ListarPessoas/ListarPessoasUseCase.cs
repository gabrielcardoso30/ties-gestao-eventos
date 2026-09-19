using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Module.Pessoas.Shared;
using Shared.Contracts.Common;
using Shared.Data.Extensions;
using Shared.Http.Endpoints;
using Shared.Http.Results;

namespace Module.Pessoas.UseCases.ListarPessoas;

internal sealed class ListarPessoasUseCase(PessoasDbContext db, ILogger<ListarPessoasUseCase> logger) : IUseCase<ListarPessoasRequest, PagedResult<ListarPessoasItemResponse>>
{
    public async Task<Result<PagedResult<ListarPessoasItemResponse>>> HandleAsync(ListarPessoasRequest request, CancellationToken cancellationToken)
    {
        var query = db.Pessoas.TagWith("Pessoas.ListarPessoas").AsNoTracking();
        logger.LogDebug("Montando listagem de pessoas: busca={HasSearch}, ativo={HasActiveFilter}", !string.IsNullOrWhiteSpace(request.Busca), request.EstaAtivo.HasValue);

        if (!string.IsNullOrWhiteSpace(request.Busca))
        {
            var busca = $"%{request.Busca.Trim()}%";
            query = query.Where(p =>
                EF.Functions.ILike(p.PessoaNome, busca) ||
                EF.Functions.ILike(p.PessoaEmail, busca) ||
                (p.PessoaEmpresa != null && EF.Functions.ILike(p.PessoaEmpresa, busca)));
            logger.LogDebug("Filtro textual aplicado à listagem de pessoas sem registrar seu conteúdo");
        }

        if (request.EstaAtivo.HasValue)
        {
            query = query.Where(p => p.EstaAtivo == request.EstaAtivo.Value);
            logger.LogDebug("Filtro de ativo={Active} aplicado à listagem de pessoas", request.EstaAtivo);
        }

        var descendente = request.Direcao == OrdenacaoDirecao.Desc;
        logger.LogDebug("Ordenando pessoas por {SortField} em direção {SortDirection}", request.OrdenarPor ?? "pessoaNome", request.Direcao);
        var ordenada = request.OrdenarPor?.ToLowerInvariant() switch
        {
            "pessoaemail" => descendente ? query.OrderByDescending(x => x.PessoaEmail).ThenByDescending(x => x.Id) : query.OrderBy(x => x.PessoaEmail).ThenBy(x => x.Id),
            "pessoaempresa" => descendente ? query.OrderByDescending(x => x.PessoaEmpresa).ThenByDescending(x => x.Id) : query.OrderBy(x => x.PessoaEmpresa).ThenBy(x => x.Id),
            "estaativo" => descendente ? query.OrderByDescending(x => x.EstaAtivo).ThenByDescending(x => x.Id) : query.OrderBy(x => x.EstaAtivo).ThenBy(x => x.Id),
            _ => descendente ? query.OrderByDescending(x => x.PessoaNome).ThenByDescending(x => x.Id) : query.OrderBy(x => x.PessoaNome).ThenBy(x => x.Id)
        };
        var pagina = await ordenada
            .Select(p => new ListarPessoasItemResponse(p.Id, p.PessoaNome, p.PessoaEmail, p.PessoaEmpresa, p.PessoaCargo, p.EstaAtivo))
            .ToPagedResultAsync(new PagedRequest(request.Pagina, request.TamanhoPagina), cancellationToken);

        logger.LogInformation("Listagem de pessoas retornou {ReturnedCount} de {TotalCount} registro(s)", pagina.Itens.Count, pagina.Total);

        return pagina;
    }
}
