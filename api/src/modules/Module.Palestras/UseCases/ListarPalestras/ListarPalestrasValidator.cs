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
        RuleFor(r => r.OrdenarPor).Must(c => new[] { "palestraTitulo", "palestraInicio", "palestrantesQuantidade", "presencasQuantidade" }.Contains(c!, StringComparer.OrdinalIgnoreCase)).When(r => !string.IsNullOrWhiteSpace(r.OrdenarPor));
    }
}
