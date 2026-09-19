using Microsoft.EntityFrameworkCore;
using Module.Palestras.Domain;
using Module.Palestras.Shared;
using Shared.Data.Extensions;
using Shared.Http.Endpoints;
using Shared.Http.Results;

namespace Module.Palestras.UseCases.RemoverConteudo;

internal sealed class RemoverConteudoUseCase(PalestrasDbContext db) : IUseCase<RemoverConteudoRequest, RemoverConteudoResponse>
{
    public async Task<Result<RemoverConteudoResponse>> HandleAsync(RemoverConteudoRequest request, CancellationToken cancellationToken)
    {
        var palestra = await db.Palestras
            .TagWith("Palestras.RemoverConteudo.Carregar")
            .Include(p => p.Conteudos)
            .FirstOrDefaultAsync(p => p.Id == request.PalestraId, cancellationToken);
        if (palestra is null)
        {
            return PalestrasErros.PalestraNaoEncontrada;
        }

        var resultado = palestra.RemoverConteudo(request.ConteudoId);
        if (resultado.IsFailure)
        {
            return resultado.Error;
        }

        return await db.ExecuteInTransactionAsync(async ct =>
        {
            db.PalestraConteudos.Remove(resultado.Value);
            await db.SaveChangesAsync(ct);
            return Result.Success(new RemoverConteudoResponse(resultado.Value.Id));
        }, cancellationToken);
    }
}
