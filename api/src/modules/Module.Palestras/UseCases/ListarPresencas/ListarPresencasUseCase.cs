using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Module.Palestras.Domain;
using Module.Palestras.Shared;
using Shared.Contracts.Pessoas;
using Shared.Http.Endpoints;
using Shared.Http.Results;

namespace Module.Palestras.UseCases.ListarPresencas;

/// <summary>Lista as presenças da palestra com nome da pessoa (lote via contrato) e indicação de certificado emitido.</summary>
internal sealed class ListarPresencasUseCase(PalestrasDbContext db, IPessoasModuleApi pessoasApi, ILogger<ListarPresencasUseCase> logger) : IUseCase<ListarPresencasRequest, IReadOnlyList<ListarPresencasItemResponse>>
{
    public async Task<Result<IReadOnlyList<ListarPresencasItemResponse>>> HandleAsync(ListarPresencasRequest request, CancellationToken cancellationToken)
    {
        var palestraExiste = await db.Palestras
            .TagWith("Palestras.ListarPresencas.VerificarPalestra")
            .AnyAsync(p => p.Id == request.PalestraId, cancellationToken);
        if (!palestraExiste)
        {
            logger.LogInformation("Listagem de presenças rejeitada: palestra {TalkId} não encontrada", request.PalestraId);
            return PalestrasErros.PalestraNaoEncontrada;
        }

        var presencas = await db.Presencas
            .TagWith("Palestras.ListarPresencas")
            .AsNoTracking()
            .Where(p => p.PalestraId == request.PalestraId)
            .OrderBy(p => p.PresencaRegistradaEm)
            .Select(p => new
            {
                p.Id,
                p.PessoaId,
                p.PresencaRegistradaEm,
                CertificadoEmitido = db.Certificados.Any(c => c.PalestraId == p.PalestraId && c.PessoaId == p.PessoaId),
            })
            .ToListAsync(cancellationToken);

        if (presencas.Count == 0)
        {
            logger.LogInformation("Palestra {TalkId} ainda não possui presenças registradas", request.PalestraId);
            return Array.Empty<ListarPresencasItemResponse>();
        }

        var pessoaIds = presencas.Select(p => p.PessoaId).Distinct().ToList();
        logger.LogDebug("Consultando módulo Pessoas em lote para enriquecer {PersonCount} presença(s) distintas", pessoaIds.Count);
        var pessoas = await pessoasApi.ObterPessoasResumoAsync(pessoaIds, cancellationToken);
        var nomes = pessoas.ToDictionary(p => p.Id, p => p.PessoaNome);
        logger.LogInformation("Palestra {TalkId} possui {AttendanceCount} presença(s), {CertificateCount} com certificado e {MissingPersonCount} pessoa(s) não localizada(s)",
            request.PalestraId, presencas.Count, presencas.Count(p => p.CertificadoEmitido), pessoaIds.Count(id => !nomes.ContainsKey(id)));

        logger.LogDebug("Mapeando {AttendanceCount} presença(s) com os resumos de pessoas", presencas.Count);
        var itens = presencas
            .Select(p => new ListarPresencasItemResponse(p.Id, p.PessoaId, nomes.GetValueOrDefault(p.PessoaId, string.Empty), p.PresencaRegistradaEm, p.CertificadoEmitido))
            .ToList();
        return itens;
    }
}
