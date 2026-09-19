using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Module.Identidade.Shared;
using Shared.Contracts.Common;
using Shared.Data.Extensions;
using Shared.Http.Endpoints;
using Shared.Http.Results;

namespace Module.Identidade.UseCases.ListarUsuarios;

/// <summary>Listagem administrativa com projeção direta (sem UserManager): perfis via join em UsuarioPerfis/Perfis.</summary>
internal sealed class ListarUsuariosUseCase(IdentidadeDbContext db, ILogger<ListarUsuariosUseCase> logger) : IUseCase<ListarUsuariosRequest, PagedResult<ListarUsuariosItemResponse>>
{
    public async Task<Result<PagedResult<ListarUsuariosItemResponse>>> HandleAsync(ListarUsuariosRequest request, CancellationToken cancellationToken)
    {
        var query = db.Usuarios.TagWith("Identidade.ListarUsuarios").AsNoTracking();
        logger.LogDebug("Montando listagem de usuários: busca={HasSearch}, ativo={HasActiveFilter}", !string.IsNullOrWhiteSpace(request.Busca), request.EstaAtivo.HasValue);

        if (!string.IsNullOrWhiteSpace(request.Busca))
        {
            var busca = $"%{request.Busca.Trim()}%";
            query = query.Where(u => EF.Functions.ILike(u.UsuarioNome, busca) || EF.Functions.ILike(u.Email!, busca));
            logger.LogDebug("Filtro textual aplicado à listagem de usuários sem registrar seu conteúdo");
        }

        if (request.EstaAtivo.HasValue)
        {
            query = query.Where(u => u.EstaAtivo == request.EstaAtivo.Value);
            logger.LogDebug("Filtro de ativo={Active} aplicado à listagem de usuários", request.EstaAtivo);
        }

        var descendente = request.Direcao == OrdenacaoDirecao.Desc;
        logger.LogDebug("Ordenando usuários por {SortField} em direção {SortDirection}", request.OrdenarPor ?? "usuarioNome", request.Direcao);
        var ordenada = request.OrdenarPor?.ToLowerInvariant() switch
        {
            "usuarioemail" => descendente ? query.OrderByDescending(x => x.Email).ThenByDescending(x => x.Id) : query.OrderBy(x => x.Email).ThenBy(x => x.Id),
            "estaativo" => descendente ? query.OrderByDescending(x => x.EstaAtivo).ThenByDescending(x => x.Id) : query.OrderBy(x => x.EstaAtivo).ThenBy(x => x.Id),
            "ultimoacessoem" => descendente ? query.OrderByDescending(x => x.UltimoAcessoEm).ThenByDescending(x => x.Id) : query.OrderBy(x => x.UltimoAcessoEm).ThenBy(x => x.Id),
            _ => descendente ? query.OrderByDescending(x => x.UsuarioNome).ThenByDescending(x => x.Id) : query.OrderBy(x => x.UsuarioNome).ThenBy(x => x.Id)
        };
        var pagina = await ordenada
            .Select(u => new ListarUsuariosItemResponse(u.Id, u.UsuarioNome, u.Email ?? string.Empty, db.UsuarioPerfis.Where(up => up.UserId == u.Id).Join(db.Perfis, up => up.RoleId, p => p.Id, (up, p) => p.Name!).OrderBy(nome => nome).ToList(), u.EstaAtivo, u.UltimoAcessoEm))
            .ToPagedResultAsync(new PagedRequest(request.Pagina, request.TamanhoPagina), cancellationToken);

        logger.LogInformation("Listagem de usuários retornou {ReturnedCount} de {TotalCount} registro(s)", pagina.Itens.Count, pagina.Total);

        return pagina;
    }
}
