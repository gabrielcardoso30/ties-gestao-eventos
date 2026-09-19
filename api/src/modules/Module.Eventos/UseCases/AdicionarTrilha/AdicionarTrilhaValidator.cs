using FluentValidation;
using Shared.Http.Validation;
namespace Module.Eventos.UseCases.AdicionarTrilha;
internal sealed class AdicionarTrilhaValidator : AbstractValidator<AdicionarTrilhaRequest>
{
    public AdicionarTrilhaValidator()
    {
        RuleFor(x => x.TrilhaNome).NotEmpty().WithMessage(MensagensValidacao.Obrigatorio).MaximumLength(120).WithMessage(MensagensValidacao.TamanhoMaximo);
        RuleFor(x => x.TrilhaDescricao).MaximumLength(1000).WithMessage(MensagensValidacao.TamanhoMaximo);
        RuleFor(x => x.TrilhaCor).Matches("^#[0-9A-Fa-f]{6}$").When(x => !string.IsNullOrWhiteSpace(x.TrilhaCor)).WithMessage("Informe uma cor hexadecimal no formato #RRGGBB.");
    }
}
