using Microsoft.EntityFrameworkCore;
using Module.Eventos.Domain;
using Module.Eventos.Shared;
using Npgsql;
using Shared.Contracts.Locais;
using Shared.Contracts.Pessoas;
using Shared.Data.Extensions;
using Shared.Http.Endpoints;
using Shared.Http.Results;

namespace Module.Eventos.UseCases.InscreverParticipante;

/// <summary>
/// Inscreve uma pessoa em um evento publicado/em andamento respeitando a capacidade. A contagem de confirmadas é uma consulta
/// projetada (não carrega a coleção); a corrida entre requisições concorrentes é resolvida pelo índice único filtrado do banco.
/// </summary>
internal sealed class InscreverParticipanteUseCase(
    EventosDbContext db,
    IPessoasModuleApi pessoas,
    ILocaisModuleApi locais,
    TimeProvider timeProvider) : IUseCase<InscreverParticipanteRequest, InscreverParticipanteResponse>
{
    public async Task<Result<InscreverParticipanteResponse>> HandleAsync(InscreverParticipanteRequest request, CancellationToken cancellationToken)
    {
        var evento = await db.Eventos
            .TagWith("Eventos.InscreverParticipante.CarregarEvento")
            .FirstOrDefaultAsync(e => e.Id == request.EventoId, cancellationToken);
        if (evento is null)
        {
            return EventosErros.EventoNaoEncontrado;
        }

        if (!evento.AceitaInscricoes)
        {
            return EventosErros.EventoNaoAceitaInscricoes;
        }

        var pessoa = await pessoas.ObterPessoaResumoAsync(request.PessoaId, cancellationToken);
        if (pessoa is null)
        {
            return EventosErros.PessoaNaoEncontrada;
        }

        var jaInscrita = await db.Inscricoes
            .TagWith("Eventos.InscreverParticipante.VerificarInscricaoConfirmada")
            .AnyAsync(i => i.EventoId == evento.Id && i.PessoaId == request.PessoaId && i.InscricaoSituacao == InscricaoSituacao.Confirmada, cancellationToken);
        if (jaInscrita)
        {
            return EventosErros.PessoaJaInscrita;
        }

        var confirmadas = await db.Inscricoes
            .TagWith("Eventos.InscreverParticipante.ContarConfirmadas")
            .CountAsync(i => i.EventoId == evento.Id && i.InscricaoSituacao == InscricaoSituacao.Confirmada, cancellationToken);

        int? localCapacidadeTotal = null;
        if (evento.EventoCapacidadeMaxima is null && evento.LocalId.HasValue)
        {
            var local = await locais.ObterLocalResumoAsync(evento.LocalId.Value, cancellationToken);
            localCapacidadeTotal = local?.LocalCapacidadeTotal;
        }

        var resultado = evento.Inscrever(request.PessoaId, confirmadas, localCapacidadeTotal, timeProvider.GetUtcNow());
        if (resultado.IsFailure)
        {
            return resultado.Error;
        }

        var inscricao = resultado.Value;
        try
        {
            return await db.ExecuteInTransactionAsync(async ct =>
            {
                db.Inscricoes.Add(inscricao);
                await db.SaveChangesAsync(ct);
                return Result.Success(new InscreverParticipanteResponse(inscricao.Id, inscricao.EventoId, inscricao.PessoaId, inscricao.InscricaoSituacao, inscricao.InscricaoRealizadaEm));
            }, cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            // Duas requisições passaram pela verificação ao mesmo tempo; o índice único filtrado garantiu uma só inscrição confirmada.
            return EventosErros.PessoaJaInscrita;
        }
    }
}
