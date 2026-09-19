using FluentValidation;
using Module.Palestras.Domain;
using Shared.Http.Validation;

namespace Module.Palestras.UseCases.ValidarCertificado;

internal sealed class ValidarCertificadoValidator : AbstractValidator<ValidarCertificadoRequest>
{
    public ValidarCertificadoValidator() =>
        RuleFor(r => r.CertificadoCodigo).NotEmpty().WithMessage(MensagensValidacao.Obrigatorio).MaximumLength(CertificadoCodigoGerador.Tamanho * 2).WithMessage(MensagensValidacao.TamanhoMaximo);
}
