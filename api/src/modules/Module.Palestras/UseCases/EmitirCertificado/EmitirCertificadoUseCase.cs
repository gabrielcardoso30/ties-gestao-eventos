using Microsoft.EntityFrameworkCore;
using Module.Palestras.Domain;
using Module.Palestras.Shared;
using Shared.Contracts.Palestras;
using Shared.Contracts.Pessoas;
using Shared.Data.Extensions;
using Shared.Http.Endpoints;
using Shared.Http.Results;

namespace Module.Palestras.UseCases.EmitirCertificado;

/// <summary>Emite (ou devolve o já emitido) o certificado de uma pessoa com presença na palestra encerrada. Emite <see cref="CertificadoEmitido"/> quando cria.</summary>
internal sealed class EmitirCertificadoUseCase(PalestrasDbContext db, IPessoasModuleApi pessoasApi, TimeProvider timeProvider) : IUseCase<EmitirCertificadoRequest, EmitirCertificadoResponse>
{
    public async Task<Result<EmitirCertificadoResponse>> HandleAsync(EmitirCertificadoRequest request, CancellationToken cancellationToken)
    {
        var palestra = await db.Palestras
            .TagWith("Palestras.EmitirCertificado.Carregar")
            .Include(p => p.Presencas.Where(x => x.PessoaId == request.PessoaId))
            .Include(p => p.Certificados.Where(x => x.PessoaId == request.PessoaId))
            .FirstOrDefaultAsync(p => p.Id == request.PalestraId, cancellationToken);
        if (palestra is null)
        {
            return PalestrasErros.PalestraNaoEncontrada;
        }

        var resultado = palestra.EmitirCertificado(request.PessoaId, timeProvider.GetUtcNow());
        if (resultado.IsFailure)
        {
            return resultado.Error;
        }

        var (certificado, criado) = resultado.Value;
        if (criado)
        {
            var gravacao = await db.ExecuteInTransactionAsync(async ct =>
            {
                await db.SaveChangesAsync(ct);
                return Result.Success();
            }, cancellationToken);
            if (gravacao.IsFailure)
            {
                return gravacao.Error;
            }
        }

        var pessoa = await pessoasApi.ObterPessoaResumoAsync(request.PessoaId, cancellationToken);

        return new EmitirCertificadoResponse(
            certificado.Id,
            certificado.CertificadoCodigo,
            palestra.Id,
            palestra.PalestraTitulo,
            certificado.PessoaId,
            pessoa?.PessoaNome ?? string.Empty,
            certificado.CertificadoEmitidoEm,
            certificado.CertificadoCargaHorariaMinutos)
        { Criado = criado };
    }
}
