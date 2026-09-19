using Microsoft.EntityFrameworkCore;
using Module.Locais.Domain;
using Module.Locais.Shared;
using Shared.Data.Extensions;
using Shared.Http.Endpoints;
using Shared.Http.Results;

namespace Module.Locais.UseCases.AtualizarLocal;

internal sealed class AtualizarLocalUseCase(LocaisDbContext db) : IUseCase<AtualizarLocalRequest, AtualizarLocalResponse>
{
    public async Task<Result<AtualizarLocalResponse>> HandleAsync(AtualizarLocalRequest request, CancellationToken cancellationToken)
    {
        var local = await db.Locais
            .TagWith("Locais.AtualizarLocal.Carregar")
            .FirstOrDefaultAsync(l => l.Id == request.LocalId, cancellationToken);
        if (local is null)
        {
            return LocaisErros.LocalNaoEncontrado;
        }

        var nomeEmUso = await db.Locais
            .TagWith("Locais.AtualizarLocal.VerificarNome")
            .AnyAsync(l => l.Id != request.LocalId && l.LocalNome == request.LocalNome.Trim(), cancellationToken);
        if (nomeEmUso)
        {
            return LocaisErros.LocalNomeDuplicado;
        }

        local.Atualizar(request.LocalNome, request.LocalDescricao, request.EnderecoLogradouro, request.EnderecoNumero,
            request.EnderecoBairro, request.EnderecoCidade, request.EnderecoUf, request.EnderecoCep);

        return await db.ExecuteInTransactionAsync(async ct =>
        {
            await db.SaveChangesAsync(ct);
            return Result.Success(new AtualizarLocalResponse(local.Id, local.LocalNome, local.AlteradoEm));
        }, cancellationToken);
    }
}
