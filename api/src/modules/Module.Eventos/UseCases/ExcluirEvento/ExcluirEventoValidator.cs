using FluentValidation;
using Shared.Http.Validation;

namespace Module.Eventos.UseCases.ExcluirEvento;

internal sealed class ExcluirEventoValidator : AbstractValidator<ExcluirEventoRequest>
{
    public ExcluirEventoValidator() => RuleFor(r => r.EventoId).NotEmpty().WithMessage(MensagensValidacao.GuidObrigatorio);
}
