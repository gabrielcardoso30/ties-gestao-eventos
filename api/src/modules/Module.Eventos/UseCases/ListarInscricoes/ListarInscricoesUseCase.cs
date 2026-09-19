using Microsoft.EntityFrameworkCore;
using Module.Eventos.Domain;
using Module.Eventos.Shared;
using Shared.Contracts.Common;
using Shared.Contracts.Pessoas;
using Shared.Data.Extensions;
using Shared.Http.Endpoints;
using Shared.Http.Results;

namespace Module.Eventos.UseCases.ListarInscricoes;

/// <summary>Lista inscrições do evento; nomes e e-mails vêm do módulo Pessoas em uma única chamada em lote.</summary>
internal sealed class ListarInscricoesUseCase(EventosDbContext db, IPessoasModuleApi pessoas) : IUseCase<ListarInscricoesRequest, PagedResult<ListarInscricoesItemResponse>>
{
    public async Task<Result<PagedResult<ListarInscricoesItemResponse>>> HandleAsync(ListarInscricoesRequest request, CancellationToken cancellationToken)
    {
        var eventoExiste = await db.Eventos
            .TagWith("Eventos.ListarInscricoes.VerificarEvento")
            .AnyAsync(e => e.Id == request.EventoId, cancellationToken);
        if (!eventoExiste)
        {
            return EventosErros.EventoNaoEncontrado;
        }

        var query = db.Inscricoes
            .TagWith("Eventos.ListarInscricoes")
            .AsNoTracking()
            .Where(i => i.EventoId == request.EventoId);

        if (request.InscricaoSituacao.HasValue)
        {
            query = query.Where(i => i.InscricaoSituacao == request.InscricaoSituacao.Value);
        }

        var pagina = await query
            .OrderBy(i => i.InscricaoRealizadaEm)
            .Select(i => new InscricaoProjecao(i.Id, i.PessoaId, i.InscricaoSituacao, i.InscricaoRealizadaEm))
            .ToPagedResultAsync(new PagedRequest(request.Pagina, request.TamanhoPagina), cancellationToken);

        var pessoaIds = pagina.Itens.Select(i => i.PessoaId).Distinct().ToList();
        var resumos = pessoaIds.Count == 0
            ? []
            : await pessoas.ObterPessoasResumoAsync(pessoaIds, cancellationToken);
        var porId = resumos.ToDictionary(p => p.Id);

        var itens = pagina.Itens
            .Select(i =>
            {
                var pessoa = porId.GetValueOrDefault(i.PessoaId);
                return new ListarInscricoesItemResponse(i.Id, i.PessoaId, pessoa?.PessoaNome ?? string.Empty, pessoa?.PessoaEmail ?? string.Empty, i.InscricaoSituacao, i.InscricaoRealizadaEm);
            })
            .ToList();

        return new PagedResult<ListarInscricoesItemResponse>(itens, pagina.Pagina, pagina.TamanhoPagina, pagina.Total);
    }

    private sealed record InscricaoProjecao(Guid Id, Guid PessoaId, InscricaoSituacao InscricaoSituacao, DateTimeOffset InscricaoRealizadaEm);
}
