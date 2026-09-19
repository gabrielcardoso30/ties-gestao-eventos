using FluentValidation;
using Shared.Http.Validation;

namespace Module.Eventos.UseCases.CancelarInscricao;

internal sealed class CancelarInscricaoValidator : AbstractValidator<CancelarInscricaoRequest>
{
    public CancelarInscricaoValidator()
    {
        RuleFor(r => r.EventoId).NotEmpty().WithMessage(MensagensValidacao.GuidObrigatorio);
        RuleFor(r => r.InscricaoId).NotEmpty().WithMessage(MensagensValidacao.GuidObrigatorio);
    }
}
