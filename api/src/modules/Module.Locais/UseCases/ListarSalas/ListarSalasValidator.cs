using FluentValidation;
using Shared.Http.Validation;

namespace Module.Locais.UseCases.ListarSalas;

internal sealed class ListarSalasValidator : AbstractValidator<ListarSalasRequest>
{
    public ListarSalasValidator() => RuleFor(r => r.LocalId).NotEmpty().WithMessage(MensagensValidacao.GuidObrigatorio);
}
