using FluentValidation;
using Shared.Http.Validation;
namespace Module.Eventos.UseCases.ExcluirTrilha;
internal sealed class ExcluirTrilhaValidator : AbstractValidator<ExcluirTrilhaRequest>
{
    public ExcluirTrilhaValidator() { RuleFor(x => x.EventoId).NotEmpty().WithMessage(MensagensValidacao.GuidObrigatorio); RuleFor(x => x.TrilhaId).NotEmpty().WithMessage(MensagensValidacao.GuidObrigatorio); }
}
