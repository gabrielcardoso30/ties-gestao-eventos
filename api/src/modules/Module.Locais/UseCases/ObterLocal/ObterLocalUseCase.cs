using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Module.Locais.Domain;
using Module.Locais.Shared;
using Shared.Http.Endpoints;
using Shared.Http.Results;

namespace Module.Locais.UseCases.ObterLocal;

internal sealed class ObterLocalUseCase(LocaisDbContext db, ILogger<ObterLocalUseCase> logger) : IUseCase<ObterLocalRequest, ObterLocalResponse>
{
    public async Task<Result<ObterLocalResponse>> HandleAsync(ObterLocalRequest request, CancellationToken cancellationToken)
    {
        var local = await db.Locais
            .TagWith("Locais.ObterLocal")
            .AsNoTracking()
            .Where(l => l.Id == request.LocalId)
            .Select(l => new ObterLocalResponse(
                l.Id, l.LocalNome, l.LocalDescricao, l.EnderecoLogradouro, l.EnderecoNumero, l.EnderecoBairro,
                l.EnderecoCidade, l.EnderecoUf, l.EnderecoCep, l.Salas.Sum(s => s.SalaCapacidade), l.EstaAtivo, l.CriadoEm, l.AlteradoEm,
                l.Salas.OrderBy(s => s.SalaNome)
                    .Select(s => new ObterLocalSalaResponse(s.Id, s.SalaNome, s.SalaCapacidade, s.SalaTipo, s.SalaRecursos, s.EstaAtivo))
                    .ToList()))
            .FirstOrDefaultAsync(cancellationToken);

        if (local is null)
        {
            logger.LogInformation("Local {LocalId} não encontrado para detalhamento", request.LocalId);
            return LocaisErros.LocalNaoEncontrado;
        }

        logger.LogInformation("Local {LocalId} carregado com {RoomCount} sala(s) e capacidade total {TotalCapacity}", local.Id, local.Salas.Count, local.LocalCapacidadeTotal);
        return local;
    }
}
