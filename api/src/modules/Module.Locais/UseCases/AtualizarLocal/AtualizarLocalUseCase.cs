using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Module.Locais.Domain;
using Module.Locais.Shared;
using Shared.Data.Extensions;
using Shared.Http.Endpoints;
using Shared.Http.Results;

namespace Module.Locais.UseCases.AtualizarLocal;

internal sealed class AtualizarLocalUseCase(LocaisDbContext db, ILogger<AtualizarLocalUseCase> logger) : IUseCase<AtualizarLocalRequest, AtualizarLocalResponse>
{
    public async Task<Result<AtualizarLocalResponse>> HandleAsync(AtualizarLocalRequest request, CancellationToken cancellationToken)
    {
        var local = await db.Locais
            .TagWith("Locais.AtualizarLocal.Carregar")
            .FirstOrDefaultAsync(l => l.Id == request.LocalId, cancellationToken);
        if (local is null)
        {
            logger.LogInformation("Local {LocalId} não encontrado para atualização", request.LocalId);
            return LocaisErros.LocalNaoEncontrado;
        }

        var nomeEmUso = await db.Locais
            .TagWith("Locais.AtualizarLocal.VerificarNome")
            .AnyAsync(l => l.Id != request.LocalId && l.LocalNome == request.LocalNome.Trim(), cancellationToken);
        if (nomeEmUso)
        {
            logger.LogInformation("Atualização do local {LocalId} rejeitada porque o nome normalizado já está em uso", local.Id);
            return LocaisErros.LocalNomeDuplicado;
        }

        logger.LogDebug("Chamando agregado Local {LocalId} para atualizar dados cadastrais", local.Id);
        local.Atualizar(request.LocalNome, request.LocalDescricao, request.EnderecoLogradouro, request.EnderecoNumero,
            request.EnderecoBairro, request.EnderecoCidade, request.EnderecoUf, request.EnderecoCep);

        return await db.ExecuteInTransactionAsync(async ct =>
        {
            await db.SaveChangesAsync(ct);
            logger.LogInformation("Local {LocalId} atualizado", local.Id);
            return Result.Success(new AtualizarLocalResponse(local.Id, local.LocalNome, local.AlteradoEm));
        }, cancellationToken);
    }
}
