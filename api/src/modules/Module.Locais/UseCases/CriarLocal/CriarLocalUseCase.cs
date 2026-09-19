using Microsoft.EntityFrameworkCore;
using Module.Locais.Domain;
using Module.Locais.Shared;
using Shared.Data.Extensions;
using Shared.Http.Endpoints;
using Shared.Http.Results;

namespace Module.Locais.UseCases.CriarLocal;

internal sealed class CriarLocalUseCase(LocaisDbContext db) : IUseCase<CriarLocalRequest, CriarLocalResponse>
{
    public async Task<Result<CriarLocalResponse>> HandleAsync(CriarLocalRequest request, CancellationToken cancellationToken)
    {
        var nomeEmUso = await db.Locais
            .TagWith("Locais.CriarLocal.VerificarNome")
            .AnyAsync(l => l.LocalNome == request.LocalNome.Trim(), cancellationToken);
        if (nomeEmUso)
        {
            return LocaisErros.LocalNomeDuplicado;
        }

        var local = Local.Criar(
            request.LocalNome, request.LocalDescricao, request.EnderecoLogradouro, request.EnderecoNumero,
            request.EnderecoBairro, request.EnderecoCidade, request.EnderecoUf, request.EnderecoCep, request.CapacidadeAmbienteUnico);

        return await db.ExecuteInTransactionAsync(async ct =>
        {
            db.Locais.Add(local);
            await db.SaveChangesAsync(ct);
            return Result.Success(new CriarLocalResponse(local.Id, local.LocalNome, local.Salas.Count));
        }, cancellationToken);
    }
}
