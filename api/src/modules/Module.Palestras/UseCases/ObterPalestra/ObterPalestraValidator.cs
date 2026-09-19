using FluentValidation;
using Shared.Http.Validation;

namespace Module.Palestras.UseCases.ObterPalestra;

internal sealed class ObterPalestraValidator : AbstractValidator<ObterPalestraRequest>
{
    public ObterPalestraValidator() => RuleFor(r => r.PalestraId).NotEmpty().WithMessage(MensagensValidacao.GuidObrigatorio);
}
