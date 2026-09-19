using FluentValidation;
using Shared.Http.Validation;

namespace Module.Palestras.UseCases.ExcluirPalestra;

internal sealed class ExcluirPalestraValidator : AbstractValidator<ExcluirPalestraRequest>
{
    public ExcluirPalestraValidator() => RuleFor(r => r.PalestraId).NotEmpty().WithMessage(MensagensValidacao.GuidObrigatorio);
}
