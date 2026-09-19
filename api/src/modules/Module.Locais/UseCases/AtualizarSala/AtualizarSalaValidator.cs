using FluentValidation;
using Shared.Http.Validation;

namespace Module.Locais.UseCases.AtualizarSala;

internal sealed class AtualizarSalaValidator : AbstractValidator<AtualizarSalaRequest>
{
    public AtualizarSalaValidator()
    {
        RuleFor(r => r.SalaNome).NotEmpty().WithMessage(MensagensValidacao.Obrigatorio).MaximumLength(100).WithMessage(MensagensValidacao.TamanhoMaximo);
        RuleFor(r => r.SalaCapacidade).GreaterThan(0).WithMessage(MensagensValidacao.MaiorQueZero);
        RuleFor(r => r.SalaTipo).IsInEnum();
        RuleFor(r => r.SalaRecursos).MaximumLength(500).WithMessage(MensagensValidacao.TamanhoMaximo);
    }
}
