using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Module.Eventos.Domain;
using Module.Eventos.Shared;
using Shared.Contracts.Common;
using Shared.Contracts.Pessoas;
using Shared.Data.Extensions;
using Shared.Http.Endpoints;
using Shared.Http.Results;

namespace Module.Eventos.UseCases.ListarInscricoes;

/// <summary>Lista inscrições do evento; nomes e e-mails vêm do módulo Pessoas em uma única chamada em lote.</summary>
internal sealed class ListarInscricoesUseCase(EventosDbContext db, IPessoasModuleApi pessoas, ILogger<ListarInscricoesUseCase> logger) : IUseCase<ListarInscricoesRequest, PagedResult<ListarInscricoesItemResponse>>
{
    public async Task<Result<PagedResult<ListarInscricoesItemResponse>>> HandleAsync(ListarInscricoesRequest request, CancellationToken cancellationToken)
    {
        var eventoExiste = await db.Eventos
            .TagWith("Eventos.ListarInscricoes.VerificarEvento")
            .AnyAsync(e => e.Id == request.EventoId, cancellationToken);
        if (!eventoExiste)
        {
            logger.LogInformation("Listagem de inscrições rejeitada: evento {EventoId} não encontrado", request.EventoId);
            return EventosErros.EventoNaoEncontrado;
        }

        var query = db.Inscricoes
            .TagWith("Eventos.ListarInscricoes")
            .AsNoTracking()
            .Where(i => i.EventoId == request.EventoId);

        if (request.InscricaoSituacao.HasValue)
        {
            logger.LogDebug("Aplicando filtro de situação {RegistrationStatus} às inscrições do evento {EventoId}", request.InscricaoSituacao, request.EventoId);
            query = query.Where(i => i.InscricaoSituacao == request.InscricaoSituacao.Value);
        }
        else
        {
            logger.LogDebug("Listagem de inscrições do evento {EventoId} inclui todas as situações", request.EventoId);
        }

        var pagina = await query
            .OrderBy(i => i.InscricaoRealizadaEm)
            .Select(i => new InscricaoProjecao(i.Id, i.PessoaId, i.InscricaoSituacao, i.InscricaoRealizadaEm))
            .ToPagedResultAsync(new PagedRequest(request.Pagina, request.TamanhoPagina), cancellationToken);

        var pessoaIds = pagina.Itens.Select(i => i.PessoaId).Distinct().ToList();
        logger.LogInformation("Página de inscrições do evento {EventoId} contém {RegistrationCount} item(ns) e {PersonCount} pessoa(s) distinta(s)", request.EventoId, pagina.Itens.Count, pessoaIds.Count);
        logger.LogDebug("Consultando módulo Pessoas em lote para enriquecer {PersonCount} inscrição(ões)", pessoaIds.Count);
        IReadOnlyList<PessoaResumo> resumos;
        if (pessoaIds.Count == 0)
        {
            logger.LogDebug("Página sem inscrições; chamada ao módulo Pessoas ignorada");
            resumos = [];
        }
        else
        {
            resumos = await pessoas.ObterPessoasResumoAsync(pessoaIds, cancellationToken);
        }
        var porId = resumos.ToDictionary(p => p.Id);
        var pessoasAusentes = pessoaIds.Count(id => !porId.ContainsKey(id));
        if (pessoasAusentes > 0)
        {
            logger.LogWarning("Módulo Pessoas não retornou {MissingPersonCount} de {PersonCount} pessoa(s) referenciada(s) nas inscrições do evento {EventoId}", pessoasAusentes, pessoaIds.Count, request.EventoId);
        }
        else
        {
            logger.LogDebug("Todas as {PersonCount} pessoa(s) das inscrições foram localizadas", pessoaIds.Count);
        }

        logger.LogDebug("Mapeando {RegistrationCount} inscrição(ões) com os resumos de pessoas", pagina.Itens.Count);
        var itens = pagina.Itens
            .Select(i =>
            {
                var pessoa = porId.GetValueOrDefault(i.PessoaId);
                return new ListarInscricoesItemResponse(i.Id, i.PessoaId, pessoa?.PessoaNome ?? string.Empty, pessoa?.PessoaEmail ?? string.Empty, i.InscricaoSituacao, i.InscricaoRealizadaEm);
            })
            .ToList();

        logger.LogInformation("Listagem do evento {EventoId} produziu {RegistrationCount} inscrição(ões) enriquecida(s)", request.EventoId, itens.Count);

        return new PagedResult<ListarInscricoesItemResponse>(itens, pagina.Pagina, pagina.TamanhoPagina, pagina.Total);
    }

    private sealed record InscricaoProjecao(Guid Id, Guid PessoaId, InscricaoSituacao InscricaoSituacao, DateTimeOffset InscricaoRealizadaEm);
}
