using FluentValidation;
using Shared.Http.Validation;

namespace Module.Auditoria.UseCases.ObterRegistroAuditoria;

internal sealed class ObterRegistroAuditoriaValidator : AbstractValidator<ObterRegistroAuditoriaRequest>
{
    public ObterRegistroAuditoriaValidator() => RuleFor(r => r.RegistroId).NotEmpty().WithMessage(MensagensValidacao.GuidObrigatorio);
}
