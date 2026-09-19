using FluentValidation;
using Module.Eventos.UseCases.CriarEvento;
using Shared.Http.Validation;

namespace Module.Eventos.UseCases.AlterarSituacaoEvento;

internal sealed class AlterarSituacaoEventoValidator : AbstractValidator<AlterarSituacaoEventoRequest>
{
    public AlterarSituacaoEventoValidator()
    {
        RuleFor(r => r.EventoSituacao).IsInEnum().WithMessage(MensagensValidacaoEventos.SituacaoInvalida);
        RuleFor(r => r.Motivo).MaximumLength(1000).WithMessage(MensagensValidacao.TamanhoMaximo);
    }
}
