using Microsoft.EntityFrameworkCore;
using Module.Pessoas.Domain;
using Module.Pessoas.Shared;
using Shared.Data.Extensions;
using Shared.Http.Endpoints;
using Shared.Http.Results;

namespace Module.Pessoas.UseCases.ExcluirPessoa;

/// <summary>Exclusão lógica da pessoa (interceptor converte Remove em soft delete) com publicação de <c>PessoaExcluida</c>.</summary>
internal sealed class ExcluirPessoaUseCase(PessoasDbContext db) : IUseCase<ExcluirPessoaRequest, ExcluirPessoaResponse>
{
    public async Task<Result<ExcluirPessoaResponse>> HandleAsync(ExcluirPessoaRequest request, CancellationToken cancellationToken)
    {
        var pessoa = await db.Pessoas
            .TagWith("Pessoas.ExcluirPessoa.Carregar")
            .FirstOrDefaultAsync(p => p.Id == request.PessoaId, cancellationToken);
        if (pessoa is null)
        {
            return PessoasErros.PessoaNaoEncontrada;
        }

        pessoa.MarcarExcluida();
        return await db.ExecuteInTransactionAsync(async ct =>
        {
            db.Pessoas.Remove(pessoa);
            await db.SaveChangesAsync(ct);
            return Result.Success(new ExcluirPessoaResponse(pessoa.Id));
        }, cancellationToken);
    }
}
