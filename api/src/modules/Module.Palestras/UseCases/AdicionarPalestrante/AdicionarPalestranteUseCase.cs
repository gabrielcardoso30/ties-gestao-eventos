using Microsoft.EntityFrameworkCore;
using Module.Palestras.Domain;
using Module.Palestras.Shared;
using Shared.Contracts.Pessoas;
using Shared.Data.Extensions;
using Shared.Http.Endpoints;
using Shared.Http.Results;

namespace Module.Palestras.UseCases.AdicionarPalestrante;

internal sealed class AdicionarPalestranteUseCase(PalestrasDbContext db, IPessoasModuleApi pessoasApi) : IUseCase<AdicionarPalestranteRequest, AdicionarPalestranteResponse>
{
    public async Task<Result<AdicionarPalestranteResponse>> HandleAsync(AdicionarPalestranteRequest request, CancellationToken cancellationToken)
    {
        var palestra = await db.Palestras
            .TagWith("Palestras.AdicionarPalestrante.Carregar")
            .Include(p => p.Palestrantes)
            .FirstOrDefaultAsync(p => p.Id == request.PalestraId, cancellationToken);
        if (palestra is null)
        {
            return PalestrasErros.PalestraNaoEncontrada;
        }

        var pessoa = await pessoasApi.ObterPessoaResumoAsync(request.PessoaId, cancellationToken);
        if (pessoa is null)
        {
            return PalestrasErros.PessoaNaoEncontrada;
        }

        var resultado = palestra.AdicionarPalestrante(request.PessoaId, request.PalestrantePapel);
        if (resultado.IsFailure)
        {
            return resultado.Error;
        }

        var palestrante = resultado.Value;
        return await db.ExecuteInTransactionAsync(async ct =>
        {
            await db.SaveChangesAsync(ct);
            return Result.Success(new AdicionarPalestranteResponse(palestrante.PalestraId, palestrante.PessoaId, palestrante.PalestrantePapel));
        }, cancellationToken);
    }
}
