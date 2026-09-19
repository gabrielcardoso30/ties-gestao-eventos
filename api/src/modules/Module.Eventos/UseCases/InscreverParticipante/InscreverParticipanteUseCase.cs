using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
    TimeProvider timeProvider,
    ILogger<InscreverParticipanteUseCase> logger) : IUseCase<InscreverParticipanteRequest, InscreverParticipanteResponse>
{
    public async Task<Result<InscreverParticipanteResponse>> HandleAsync(InscreverParticipanteRequest request, CancellationToken cancellationToken)
    {
        var evento = await db.Eventos
            .TagWith("Eventos.InscreverParticipante.CarregarEvento")
            .FirstOrDefaultAsync(e => e.Id == request.EventoId, cancellationToken);
        if (evento is null)
        {
            logger.LogInformation("Inscrição rejeitada: evento {EventoId} não encontrado", request.EventoId);
            return EventosErros.EventoNaoEncontrado;
        }

        if (!evento.AceitaInscricoes)
        {
            logger.LogInformation("Inscrição rejeitada: evento {EventoId} em situação {EventoSituacao} não aceita inscrições", evento.Id, evento.EventoSituacao);
            return EventosErros.EventoNaoAceitaInscricoes;
        }

        logger.LogDebug("Consultando módulo Pessoas para validar participante {PessoaId}", request.PessoaId);
        var pessoa = await pessoas.ObterPessoaResumoAsync(request.PessoaId, cancellationToken);
        if (pessoa is null)
        {
            logger.LogInformation("Inscrição no evento {EventoId} rejeitada: pessoa {PessoaId} não encontrada", evento.Id, request.PessoaId);
            return EventosErros.PessoaNaoEncontrada;
        }

        var jaInscrita = await db.Inscricoes
            .TagWith("Eventos.InscreverParticipante.VerificarInscricaoConfirmada")
            .AnyAsync(i => i.EventoId == evento.Id && i.PessoaId == request.PessoaId && i.InscricaoSituacao == InscricaoSituacao.Confirmada, cancellationToken);
        if (jaInscrita)
        {
            logger.LogInformation("Inscrição duplicada rejeitada para pessoa {PessoaId} no evento {EventoId}", request.PessoaId, evento.Id);
            return EventosErros.PessoaJaInscrita;
        }

        var confirmadas = await db.Inscricoes
            .TagWith("Eventos.InscreverParticipante.ContarConfirmadas")
            .CountAsync(i => i.EventoId == evento.Id && i.InscricaoSituacao == InscricaoSituacao.Confirmada, cancellationToken);
        logger.LogInformation("Evento {EventoId} possui {ConfirmedRegistrations} inscrição(ões) confirmada(s) antes da nova inscrição", evento.Id, confirmadas);

        int? localCapacidadeTotal = null;
        if (evento.EventoCapacidadeMaxima is null && evento.LocalId.HasValue)
        {
            logger.LogDebug("Consultando módulo Locais para obter capacidade do local {LocalId}", evento.LocalId);
            var local = await locais.ObterLocalResumoAsync(evento.LocalId.Value, cancellationToken);
            localCapacidadeTotal = local?.LocalCapacidadeTotal;
            logger.LogInformation("Capacidade considerada para o evento {EventoId}: {CapacitySource}={Capacity}", evento.Id, "Local", localCapacidadeTotal);
        }
        else
        {
            logger.LogInformation("Capacidade considerada para o evento {EventoId}: {CapacitySource}={Capacity}", evento.Id, "Evento", evento.EventoCapacidadeMaxima);
        }

        logger.LogDebug("Chamando agregado Evento {EventoId}.Inscrever para pessoa {PessoaId}, confirmadas={ConfirmedRegistrations}, capacidade={Capacity}", evento.Id, request.PessoaId, confirmadas, localCapacidadeTotal ?? evento.EventoCapacidadeMaxima);
        var resultado = evento.Inscrever(request.PessoaId, confirmadas, localCapacidadeTotal, timeProvider.GetUtcNow());
        if (resultado.IsFailure)
        {
            logger.LogInformation("Inscrição da pessoa {PessoaId} no evento {EventoId} rejeitada pela regra {ErrorCode}", request.PessoaId, evento.Id, resultado.Error.Code);
            return resultado.Error;
        }

        var inscricao = resultado.Value;
        try
        {
            return await db.ExecuteInTransactionAsync(async ct =>
            {
                db.Inscricoes.Add(inscricao);
                await db.SaveChangesAsync(ct);
                logger.LogInformation("Inscrição {RegistrationId} confirmada para pessoa {PessoaId} no evento {EventoId}", inscricao.Id, inscricao.PessoaId, inscricao.EventoId);
                return Result.Success(new InscreverParticipanteResponse(inscricao.Id, inscricao.EventoId, inscricao.PessoaId, inscricao.InscricaoSituacao, inscricao.InscricaoRealizadaEm));
            }, cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            // Duas requisições passaram pela verificação ao mesmo tempo; o índice único filtrado garantiu uma só inscrição confirmada.
            logger.LogWarning("Concorrência detectada ao inscrever pessoa {PessoaId} no evento {EventoId}; índice único preservou a regra de inscrição única", request.PessoaId, evento.Id);
            return EventosErros.PessoaJaInscrita;
        }
    }
}
