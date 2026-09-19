using Microsoft.EntityFrameworkCore;
using Module.Palestras.Domain;
using Module.Palestras.Shared;
using Shared.Data.Extensions;
using Shared.Http.Endpoints;
using Shared.Http.Results;

namespace Module.Palestras.UseCases.AdicionarConteudo;

internal sealed class AdicionarConteudoUseCase(PalestrasDbContext db) : IUseCase<AdicionarConteudoRequest, AdicionarConteudoResponse>
{
    public async Task<Result<AdicionarConteudoResponse>> HandleAsync(AdicionarConteudoRequest request, CancellationToken cancellationToken)
    {
        var palestra = await db.Palestras
            .TagWith("Palestras.AdicionarConteudo.Carregar")
            .Include(p => p.Conteudos)
            .FirstOrDefaultAsync(p => p.Id == request.PalestraId, cancellationToken);
        if (palestra is null)
        {
            return PalestrasErros.PalestraNaoEncontrada;
        }

        var conteudo = palestra.AdicionarConteudo(request.ConteudoTitulo, request.ConteudoTipo, request.ConteudoUrl, request.ConteudoDescricao);

        return await db.ExecuteInTransactionAsync(async ct =>
        {
            await db.SaveChangesAsync(ct);
            return Result.Success(new AdicionarConteudoResponse(conteudo.Id, conteudo.PalestraId, conteudo.ConteudoTitulo, conteudo.ConteudoTipo, conteudo.ConteudoUrl));
        }, cancellationToken);
    }
}
