using Microsoft.EntityFrameworkCore;
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
    IPessoasModuleApi pessoasApi) : IUseCase<CriarPalestraRequest, CriarPalestraResponse>
{
    public async Task<Result<CriarPalestraResponse>> HandleAsync(CriarPalestraRequest request, CancellationToken cancellationToken)
    {
        var verificacao = await agenda.VerificarAsync(request.EventoId, request.SalaId, request.PalestraInicio, request.PalestraFim, cancellationToken);
        if (verificacao.IsFailure)
        {
            return verificacao.Error;
        }

        if (request.SalaId.HasValue)
        {
            var salaOcupada = await db.Palestras
                .TagWith("Palestras.CriarPalestra.VerificarSala")
                .AnyAsync(PalestraAgenda.OcupaSala(request.SalaId.Value, request.PalestraInicio, request.PalestraFim), cancellationToken);
            if (salaOcupada)
            {
                return PalestrasErros.SalaOcupada;
            }
        }

        var pessoaIds = request.Palestrantes.Select(p => p.PessoaId).Distinct().ToList();
        var pessoas = await pessoasApi.ObterPessoasResumoAsync(pessoaIds, cancellationToken);
        if (pessoaIds.Any(id => pessoas.All(p => p.Id != id)))
        {
            return PalestrasErros.PessoaNaoEncontrada;
        }

        var resultado = Palestra.Criar(
            request.EventoId, request.SalaId, request.PalestraTitulo, request.PalestraDescricao, request.PalestraInicio, request.PalestraFim,
            request.Palestrantes.Select(p => new NovoPalestrante(p.PessoaId, p.PalestrantePapel)).ToList());
        if (resultado.IsFailure)
        {
            return resultado.Error;
        }

        var palestra = resultado.Value;
        return await db.ExecuteInTransactionAsync(async ct =>
        {
            db.Palestras.Add(palestra);
            await db.SaveChangesAsync(ct);
            return Result.Success(new CriarPalestraResponse(palestra.Id, palestra.EventoId, palestra.PalestraTitulo));
        }, cancellationToken);
    }
}
