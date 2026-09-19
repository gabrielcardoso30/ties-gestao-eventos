using Microsoft.EntityFrameworkCore;
using Module.Pessoas.Domain;
using Module.Pessoas.Shared;
using Shared.Http.Endpoints;
using Shared.Http.Results;

namespace Module.Pessoas.UseCases.ObterPessoa;

internal sealed class ObterPessoaUseCase(PessoasDbContext db) : IUseCase<ObterPessoaRequest, ObterPessoaResponse>
{
    public async Task<Result<ObterPessoaResponse>> HandleAsync(ObterPessoaRequest request, CancellationToken cancellationToken)
    {
        var pessoa = await db.Pessoas
            .TagWith("Pessoas.ObterPessoa")
            .AsNoTracking()
            .Where(p => p.Id == request.PessoaId)
            .Select(p => new ObterPessoaResponse(
                p.Id, p.PessoaNome, p.PessoaEmail, p.PessoaTelefone, p.PessoaDocumento, p.PessoaEmpresa, p.PessoaCargo,
                p.PessoaMiniBio, p.PessoaFotoUrl, p.EstaAtivo, p.CriadoEm, p.AlteradoEm))
            .FirstOrDefaultAsync(cancellationToken);

        return pessoa is null ? PessoasErros.PessoaNaoEncontrada : pessoa;
    }
}
