using FluentValidation;
using Shared.Contracts.Common;

namespace Module.Identidade.UseCases.ListarUsuarios;

internal sealed class ListarUsuariosValidator : AbstractValidator<ListarUsuariosRequest>
{
    public ListarUsuariosValidator()
    {
        RuleFor(r => r.Pagina).GreaterThanOrEqualTo(1);
        RuleFor(r => r.TamanhoPagina).InclusiveBetween(1, PagedRequest.TamanhoMaximo);
        RuleFor(r => r.Busca).MaximumLength(100);
    }
}
