using FluentValidation;
using Shared.Http.Validation;

namespace Module.Eventos.UseCases.ObterEvento;

internal sealed class ObterEventoValidator : AbstractValidator<ObterEventoRequest>
{
    public ObterEventoValidator() => RuleFor(r => r.EventoId).NotEmpty().WithMessage(MensagensValidacao.GuidObrigatorio);
}
