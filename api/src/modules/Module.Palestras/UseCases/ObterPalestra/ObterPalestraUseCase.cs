using Microsoft.EntityFrameworkCore;
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
    IPessoasModuleApi pessoasApi) : IUseCase<ObterPalestraRequest, ObterPalestraResponse>
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
            return PalestrasErros.PalestraNaoEncontrada;
        }

        var evento = await eventosApi.ObterEventoResumoAsync(palestra.EventoId, cancellationToken);
        var sala = palestra.SalaId.HasValue ? await locaisApi.ObterSalaResumoAsync(palestra.SalaId.Value, cancellationToken) : null;
        var pessoas = await pessoasApi.ObterPessoasResumoAsync(palestra.Palestrantes.Select(x => x.PessoaId).Distinct().ToList(), cancellationToken);
        var nomes = pessoas.ToDictionary(p => p.Id, p => p.PessoaNome);

        return new ObterPalestraResponse(
            palestra.Id,
            palestra.EventoId,
            evento?.EventoNome ?? string.Empty,
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
