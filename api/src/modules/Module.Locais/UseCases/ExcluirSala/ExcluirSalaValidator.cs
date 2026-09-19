using FluentValidation;
using Shared.Http.Validation;

namespace Module.Locais.UseCases.ExcluirSala;

internal sealed class ExcluirSalaValidator : AbstractValidator<ExcluirSalaRequest>
{
    public ExcluirSalaValidator()
    {
        RuleFor(r => r.LocalId).NotEmpty().WithMessage(MensagensValidacao.GuidObrigatorio);
        RuleFor(r => r.SalaId).NotEmpty().WithMessage(MensagensValidacao.GuidObrigatorio);
    }
}
