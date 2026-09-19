using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Module.Locais.Domain;
using Module.Locais.Shared;
using Shared.Data.Extensions;
using Shared.Http.Endpoints;
using Shared.Http.Results;

namespace Module.Locais.UseCases.CriarLocal;

internal sealed class CriarLocalUseCase(LocaisDbContext db, ILogger<CriarLocalUseCase> logger) : IUseCase<CriarLocalRequest, CriarLocalResponse>
{
    public async Task<Result<CriarLocalResponse>> HandleAsync(CriarLocalRequest request, CancellationToken cancellationToken)
    {
        var nomeEmUso = await db.Locais
            .TagWith("Locais.CriarLocal.VerificarNome")
            .AnyAsync(l => l.LocalNome == request.LocalNome.Trim(), cancellationToken);
        if (nomeEmUso)
        {
            logger.LogInformation("Criação de local rejeitada porque o nome normalizado já está em uso");
            return LocaisErros.LocalNomeDuplicado;
        }

        logger.LogDebug("Chamando fábrica de domínio Local.Criar; ambiente único={SingleEnvironment}", request.CapacidadeAmbienteUnico.HasValue);
        var local = Local.Criar(
            request.LocalNome, request.LocalDescricao, request.EnderecoLogradouro, request.EnderecoNumero,
            request.EnderecoBairro, request.EnderecoCidade, request.EnderecoUf, request.EnderecoCep, request.CapacidadeAmbienteUnico);

        return await db.ExecuteInTransactionAsync(async ct =>
        {
            db.Locais.Add(local);
            await db.SaveChangesAsync(ct);
            logger.LogInformation("Local {LocalId} criado com {RoomCount} sala(s)", local.Id, local.Salas.Count);
            return Result.Success(new CriarLocalResponse(local.Id, local.LocalNome, local.Salas.Count));
        }, cancellationToken);
    }
}
