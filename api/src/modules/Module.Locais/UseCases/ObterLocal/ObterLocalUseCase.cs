using Microsoft.EntityFrameworkCore;
using Module.Locais.Domain;
using Module.Locais.Shared;
using Shared.Http.Endpoints;
using Shared.Http.Results;

namespace Module.Locais.UseCases.ObterLocal;

internal sealed class ObterLocalUseCase(LocaisDbContext db) : IUseCase<ObterLocalRequest, ObterLocalResponse>
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

        return local is null ? LocaisErros.LocalNaoEncontrado : local;
    }
}
