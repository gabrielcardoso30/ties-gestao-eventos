using Microsoft.EntityFrameworkCore;
using Module.Palestras.Domain;
using Module.Palestras.Shared;
using Shared.Contracts.Pessoas;
using Shared.Http.Endpoints;
using Shared.Http.Results;

namespace Module.Palestras.UseCases.ListarPresencas;

/// <summary>Lista as presenças da palestra com nome da pessoa (lote via contrato) e indicação de certificado emitido.</summary>
internal sealed class ListarPresencasUseCase(PalestrasDbContext db, IPessoasModuleApi pessoasApi) : IUseCase<ListarPresencasRequest, IReadOnlyList<ListarPresencasItemResponse>>
{
    public async Task<Result<IReadOnlyList<ListarPresencasItemResponse>>> HandleAsync(ListarPresencasRequest request, CancellationToken cancellationToken)
    {
        var palestraExiste = await db.Palestras
            .TagWith("Palestras.ListarPresencas.VerificarPalestra")
            .AnyAsync(p => p.Id == request.PalestraId, cancellationToken);
        if (!palestraExiste)
        {
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
            return Array.Empty<ListarPresencasItemResponse>();
        }

        var pessoas = await pessoasApi.ObterPessoasResumoAsync(presencas.Select(p => p.PessoaId).Distinct().ToList(), cancellationToken);
        var nomes = pessoas.ToDictionary(p => p.Id, p => p.PessoaNome);

        return presencas
            .Select(p => new ListarPresencasItemResponse(p.Id, p.PessoaId, nomes.GetValueOrDefault(p.PessoaId, string.Empty), p.PresencaRegistradaEm, p.CertificadoEmitido))
            .ToList();
    }
}
