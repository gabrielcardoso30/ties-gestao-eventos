using Microsoft.EntityFrameworkCore;
using Module.Palestras.Domain;
using Module.Palestras.Shared;
using Shared.Data.Extensions;
using Shared.Http.Endpoints;
using Shared.Http.Results;

namespace Module.Palestras.UseCases.AtualizarPalestra;

/// <summary>Atualiza dados e agenda da palestra, revalidando evento, período e sala (ignorando a própria palestra na sobreposição).</summary>
internal sealed class AtualizarPalestraUseCase(PalestrasDbContext db, AgendaPalestraVerificador agenda) : IUseCase<AtualizarPalestraRequest, AtualizarPalestraResponse>
{
    public async Task<Result<AtualizarPalestraResponse>> HandleAsync(AtualizarPalestraRequest request, CancellationToken cancellationToken)
    {
        var palestra = await db.Palestras
            .TagWith("Palestras.AtualizarPalestra.Carregar")
            .FirstOrDefaultAsync(p => p.Id == request.PalestraId, cancellationToken);
        if (palestra is null)
        {
            return PalestrasErros.PalestraNaoEncontrada;
        }

        var verificacao = await agenda.VerificarAsync(palestra.EventoId, request.SalaId, request.PalestraInicio, request.PalestraFim, cancellationToken);
        if (verificacao.IsFailure)
        {
            return verificacao.Error;
        }

        if (request.SalaId.HasValue)
        {
            var salaOcupada = await db.Palestras
                .TagWith("Palestras.AtualizarPalestra.VerificarSala")
                .AnyAsync(PalestraAgenda.OcupaSala(request.SalaId.Value, request.PalestraInicio, request.PalestraFim, palestra.Id), cancellationToken);
            if (salaOcupada)
            {
                return PalestrasErros.SalaOcupada;
            }
        }

        palestra.Atualizar(request.SalaId, request.PalestraTitulo, request.PalestraDescricao, request.PalestraInicio, request.PalestraFim);

        return await db.ExecuteInTransactionAsync(async ct =>
        {
            await db.SaveChangesAsync(ct);
            return Result.Success(new AtualizarPalestraResponse(palestra.Id, palestra.PalestraTitulo, palestra.AlteradoEm));
        }, cancellationToken);
    }
}
