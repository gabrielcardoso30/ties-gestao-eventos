using FluentValidation;
using Shared.Http.Validation;

namespace Module.Locais.UseCases.ExcluirLocal;

internal sealed class ExcluirLocalValidator : AbstractValidator<ExcluirLocalRequest>
{
    public ExcluirLocalValidator() => RuleFor(r => r.LocalId).NotEmpty().WithMessage(MensagensValidacao.GuidObrigatorio);
}
