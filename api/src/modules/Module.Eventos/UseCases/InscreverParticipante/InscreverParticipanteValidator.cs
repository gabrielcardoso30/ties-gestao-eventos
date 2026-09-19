using FluentValidation;
using Shared.Http.Validation;

namespace Module.Eventos.UseCases.InscreverParticipante;

internal sealed class InscreverParticipanteValidator : AbstractValidator<InscreverParticipanteRequest>
{
    public InscreverParticipanteValidator() => RuleFor(r => r.PessoaId).NotEmpty().WithMessage(MensagensValidacao.GuidObrigatorio);
}
