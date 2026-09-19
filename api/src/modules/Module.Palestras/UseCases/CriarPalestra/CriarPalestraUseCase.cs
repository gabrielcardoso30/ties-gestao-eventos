using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Module.Palestras.Domain;
using Module.Palestras.Shared;
using Shared.Contracts.Palestras;
using Shared.Contracts.Pessoas;
using Shared.Data.Extensions;
using Shared.Http.Endpoints;
using Shared.Http.Results;

namespace Module.Palestras.UseCases.CriarPalestra;

/// <summary>Cria a palestra validando evento, período, sala (local e agenda) e palestrantes via contratos. Emite <see cref="PalestraCriada"/>.</summary>
internal sealed class CriarPalestraUseCase(
    PalestrasDbContext db,
    AgendaPalestraVerificador agenda,
    IPessoasModuleApi pessoasApi,
    ILogger<CriarPalestraUseCase> logger) : IUseCase<CriarPalestraRequest, CriarPalestraResponse>
{
    public async Task<Result<CriarPalestraResponse>> HandleAsync(CriarPalestraRequest request, CancellationToken cancellationToken)
    {
        logger.LogDebug("Validando agenda da nova palestra no evento {EventoId}, trilha {TrackId} e sala {RoomId}", request.EventoId, request.TrilhaId, request.SalaId);
        var verificacao = await agenda.VerificarAsync(request.EventoId, request.TrilhaId, request.SalaId, request.PalestraInicio, request.PalestraFim, cancellationToken);
        if (verificacao.IsFailure)
        {
            logger.LogInformation("Agenda da nova palestra rejeitada pela regra {ErrorCode}", verificacao.Error.Code);
            return verificacao.Error;
        }

        if (request.SalaId.HasValue)
        {
            logger.LogDebug("Verificando sobreposição da sala {RoomId} para nova palestra", request.SalaId);
            var salaOcupada = await db.Palestras
                .TagWith("Palestras.CriarPalestra.VerificarSala")
                .AnyAsync(PalestraAgenda.OcupaSala(request.SalaId.Value, request.PalestraInicio, request.PalestraFim), cancellationToken);
            if (salaOcupada)
            {
                logger.LogInformation("Nova palestra rejeitada porque a sala {RoomId} está ocupada no período solicitado", request.SalaId);
                return PalestrasErros.SalaOcupada;
            }
            logger.LogDebug("Sala {RoomId} disponível no período solicitado", request.SalaId);
        }
        else
        {
            logger.LogDebug("Nova palestra não possui sala; verificação de sobreposição ignorada");
        }

        var pessoaIds = request.Palestrantes.Select(p => p.PessoaId).Distinct().ToList();
        logger.LogDebug("Consultando módulo Pessoas para validar {SpeakerCount} palestrante(s) distinto(s)", pessoaIds.Count);
        var pessoas = await pessoasApi.ObterPessoasResumoAsync(pessoaIds, cancellationToken);
        var palestrantesAusentes = pessoaIds.Count(id => pessoas.All(p => p.Id != id));
        if (palestrantesAusentes > 0)
        {
            logger.LogInformation("Nova palestra rejeitada: {MissingSpeakerCount} palestrante(s) não encontrado(s)", palestrantesAusentes);
            return PalestrasErros.PessoaNaoEncontrada;
        }
        logger.LogDebug("Todos os {SpeakerCount} palestrante(s) foram localizados no módulo Pessoas", pessoaIds.Count);

        logger.LogDebug("Chamando fábrica de domínio Palestra.Criar com {SpeakerCount} palestrante(s)", request.Palestrantes.Count);
        var resultado = Palestra.Criar(
            request.EventoId, request.TrilhaId, request.SalaId, request.PalestraTitulo, request.PalestraDescricao, request.PalestraInicio, request.PalestraFim,
            request.Palestrantes.Select(p => new NovoPalestrante(p.PessoaId, p.PalestrantePapel)).ToList());
        if (resultado.IsFailure)
        {
            logger.LogInformation("Agregado rejeitou a criação da palestra pela regra {ErrorCode}", resultado.Error.Code);
            return resultado.Error;
        }

        var palestra = resultado.Value;
        return await db.ExecuteInTransactionAsync(async ct =>
        {
            db.Palestras.Add(palestra);
            await db.SaveChangesAsync(ct);
            logger.LogInformation("Palestra {TalkId} persistida no evento {EventoId} com {SpeakerCount} palestrante(s)", palestra.Id, palestra.EventoId, pessoaIds.Count);
            return Result.Success(new CriarPalestraResponse(palestra.Id, palestra.EventoId, palestra.PalestraTitulo));
        }, cancellationToken);
    }
}
