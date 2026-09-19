using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Module.Palestras.Domain;
using Module.Palestras.Shared;
using Shared.Contracts.Eventos;
using Shared.Contracts.Locais;
using Shared.Contracts.Pessoas;
using Shared.Http.Endpoints;
using Shared.Http.Results;

namespace Module.Palestras.UseCases.ObterPalestra;

/// <summary>Projeção da palestra enriquecida com nomes de evento, sala e pessoas obtidos via contratos (uma chamada em lote para pessoas).</summary>
internal sealed class ObterPalestraUseCase(
    PalestrasDbContext db,
    IEventosModuleApi eventosApi,
    ILocaisModuleApi locaisApi,
    IPessoasModuleApi pessoasApi,
    ILogger<ObterPalestraUseCase> logger) : IUseCase<ObterPalestraRequest, ObterPalestraResponse>
{
    public async Task<Result<ObterPalestraResponse>> HandleAsync(ObterPalestraRequest request, CancellationToken cancellationToken)
    {
        var palestra = await db.Palestras
            .TagWith("Palestras.ObterPalestra")
            .AsNoTracking()
            .Where(p => p.Id == request.PalestraId)
            .Select(p => new
            {
                p.Id,
                p.EventoId,
                p.TrilhaId,
                p.SalaId,
                p.PalestraTitulo,
                p.PalestraDescricao,
                p.PalestraInicio,
                p.PalestraFim,
                Palestrantes = p.Palestrantes.OrderBy(x => x.CriadoEm).Select(x => new { x.PessoaId, x.PalestrantePapel }).ToList(),
                Conteudos = p.Conteudos.OrderBy(c => c.CriadoEm)
                    .Select(c => new ObterPalestraConteudoResponse(c.Id, c.ConteudoTitulo, c.ConteudoTipo, c.ConteudoUrl, c.ConteudoDescricao))
                    .ToList(),
                PresencasQuantidade = p.Presencas.Count,
                CertificadosQuantidade = p.Certificados.Count,
                p.CriadoEm,
                p.AlteradoEm,
            })
            .FirstOrDefaultAsync(cancellationToken);
        if (palestra is null)
        {
            logger.LogInformation("Palestra {TalkId} não encontrada para detalhamento", request.PalestraId);
            return PalestrasErros.PalestraNaoEncontrada;
        }

        logger.LogDebug("Consultando módulo Eventos para enriquecer evento {EventoId} e trilha {TrackId} da palestra {TalkId}", palestra.EventoId, palestra.TrilhaId, palestra.Id);
        var evento = await eventosApi.ObterEventoResumoAsync(palestra.EventoId, cancellationToken);
        var trilha = await eventosApi.ObterTrilhaResumoAsync(palestra.EventoId, palestra.TrilhaId, cancellationToken);
        SalaResumo? sala;
        if (palestra.SalaId.HasValue)
        {
            logger.LogDebug("Consultando módulo Locais para enriquecer sala {RoomId} da palestra {TalkId}", palestra.SalaId, palestra.Id);
            sala = await locaisApi.ObterSalaResumoAsync(palestra.SalaId.Value, cancellationToken);
        }
        else
        {
            logger.LogDebug("Palestra {TalkId} não possui sala; chamada ao módulo Locais ignorada", palestra.Id);
            sala = null;
        }
        var pessoaIds = palestra.Palestrantes.Select(x => x.PessoaId).Distinct().ToList();
        logger.LogDebug("Consultando módulo Pessoas para enriquecer {SpeakerCount} palestrante(s) da palestra {TalkId}", pessoaIds.Count, palestra.Id);
        var pessoas = await pessoasApi.ObterPessoasResumoAsync(pessoaIds, cancellationToken);
        var nomes = pessoas.ToDictionary(p => p.Id, p => p.PessoaNome);
        logger.LogInformation(
            "Palestra {TalkId} carregada com {SpeakerCount} palestrante(s), {ContentCount} conteúdo(s), {AttendanceCount} presença(s) e {CertificateCount} certificado(s); referências ausentes: evento={MissingEvent}, trilha={MissingTrack}, sala={MissingRoom}, pessoas={MissingPersonCount}",
            palestra.Id, palestra.Palestrantes.Count, palestra.Conteudos.Count, palestra.PresencasQuantidade, palestra.CertificadosQuantidade,
            evento is null, trilha is null, palestra.SalaId.HasValue && sala is null, pessoaIds.Count(id => !nomes.ContainsKey(id)));

        logger.LogDebug("Mapeando {SpeakerCount} palestrante(s) e {ContentCount} conteúdo(s) para a resposta", palestra.Palestrantes.Count, palestra.Conteudos.Count);
        return new ObterPalestraResponse(
            palestra.Id,
            palestra.EventoId,
            evento?.EventoNome ?? string.Empty,
            palestra.TrilhaId,
            trilha?.TrilhaNome ?? string.Empty,
            palestra.SalaId,
            sala?.SalaNome,
            palestra.PalestraTitulo,
            palestra.PalestraDescricao,
            palestra.PalestraInicio,
            palestra.PalestraFim,
            (int)(palestra.PalestraFim - palestra.PalestraInicio).TotalMinutes,
            palestra.Palestrantes.Select(x => new ObterPalestraPalestranteResponse(x.PessoaId, nomes.GetValueOrDefault(x.PessoaId, string.Empty), x.PalestrantePapel)).ToList(),
            palestra.Conteudos,
            palestra.PresencasQuantidade,
            palestra.CertificadosQuantidade,
            palestra.CriadoEm,
            palestra.AlteradoEm);
    }
}
