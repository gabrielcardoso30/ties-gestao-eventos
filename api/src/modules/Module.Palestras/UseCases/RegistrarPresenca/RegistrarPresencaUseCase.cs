using Microsoft.EntityFrameworkCore;
using Module.Palestras.Domain;
using Module.Palestras.Shared;
using Shared.Contracts.Eventos;
using Shared.Contracts.Palestras;
using Shared.Data.Extensions;
using Shared.Http.Endpoints;
using Shared.Http.Results;

namespace Module.Palestras.UseCases.RegistrarPresenca;

/// <summary>Registra presença de um participante com inscrição confirmada no evento. Emite <see cref="PresencaRegistrada"/>.</summary>
internal sealed class RegistrarPresencaUseCase(PalestrasDbContext db, IEventosModuleApi eventosApi, TimeProvider timeProvider) : IUseCase<RegistrarPresencaRequest, RegistrarPresencaResponse>
{
    public async Task<Result<RegistrarPresencaResponse>> HandleAsync(RegistrarPresencaRequest request, CancellationToken cancellationToken)
    {
        var palestra = await db.Palestras
            .TagWith("Palestras.RegistrarPresenca.Carregar")
            .Include(p => p.Presencas.Where(x => x.PessoaId == request.PessoaId))
            .FirstOrDefaultAsync(p => p.Id == request.PalestraId, cancellationToken);
        if (palestra is null)
        {
            return PalestrasErros.PalestraNaoEncontrada;
        }

        var inscrito = await eventosApi.InscricaoConfirmadaExisteAsync(palestra.EventoId, request.PessoaId, cancellationToken);
        if (!inscrito)
        {
            return PalestrasErros.ParticipanteNaoInscrito;
        }

        var resultado = palestra.RegistrarPresenca(request.PessoaId, timeProvider.GetUtcNow());
        if (resultado.IsFailure)
        {
            return resultado.Error;
        }

        var presenca = resultado.Value;
        return await db.ExecuteInTransactionAsync(async ct =>
        {
            await db.SaveChangesAsync(ct);
            return Result.Success(new RegistrarPresencaResponse(presenca.Id, presenca.PalestraId, presenca.PessoaId, presenca.PresencaRegistradaEm));
        }, cancellationToken);
    }
}
