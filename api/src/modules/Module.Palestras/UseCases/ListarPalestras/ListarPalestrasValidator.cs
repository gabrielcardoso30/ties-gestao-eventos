using FluentValidation;
using Shared.Contracts.Common;

namespace Module.Palestras.UseCases.ListarPalestras;

internal sealed class ListarPalestrasValidator : AbstractValidator<ListarPalestrasRequest>
{
    public ListarPalestrasValidator()
    {
        RuleFor(r => r.Pagina).GreaterThanOrEqualTo(1);
        RuleFor(r => r.TamanhoPagina).InclusiveBetween(1, PagedRequest.TamanhoMaximo);
        RuleFor(r => r.Busca).MaximumLength(100);
    }
}
