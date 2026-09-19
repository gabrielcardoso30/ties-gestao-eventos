using FluentValidation;
using Shared.Http.Validation;

namespace Module.Palestras.UseCases.EmitirCertificado;

internal sealed class EmitirCertificadoValidator : AbstractValidator<EmitirCertificadoRequest>
{
    public EmitirCertificadoValidator()
    {
        RuleFor(r => r.PalestraId).NotEmpty().WithMessage(MensagensValidacao.GuidObrigatorio);
        RuleFor(r => r.PessoaId).NotEmpty().WithMessage(MensagensValidacao.GuidObrigatorio);
    }
}
