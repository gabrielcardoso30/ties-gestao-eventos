using FluentValidation;
using Shared.Contracts.Common;

namespace Module.Pessoas.UseCases.ListarPessoas;

internal sealed class ListarPessoasValidator : AbstractValidator<ListarPessoasRequest>
{
    public ListarPessoasValidator()
    {
        RuleFor(r => r.Pagina).GreaterThanOrEqualTo(1);
        RuleFor(r => r.TamanhoPagina).InclusiveBetween(1, PagedRequest.TamanhoMaximo);
        RuleFor(r => r.Busca).MaximumLength(100);
        RuleFor(r => r.OrdenarPor).Must(c => new[] { "pessoaNome", "pessoaEmail", "pessoaEmpresa", "estaAtivo" }.Contains(c!, StringComparer.OrdinalIgnoreCase)).When(r => !string.IsNullOrWhiteSpace(r.OrdenarPor));
    }
}
