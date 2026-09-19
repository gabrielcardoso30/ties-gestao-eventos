using FluentValidation;
using Shared.Http.Validation;

namespace Module.Locais.UseCases.ObterLocal;

internal sealed class ObterLocalValidator : AbstractValidator<ObterLocalRequest>
{
    public ObterLocalValidator() => RuleFor(r => r.LocalId).NotEmpty().WithMessage(MensagensValidacao.GuidObrigatorio);
}
