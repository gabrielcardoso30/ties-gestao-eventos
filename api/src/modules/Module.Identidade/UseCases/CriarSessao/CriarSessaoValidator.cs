using FluentValidation;
using Shared.Http.Validation;

namespace Module.Identidade.UseCases.CriarSessao;

internal sealed class CriarSessaoValidator : AbstractValidator<CriarSessaoRequest>
{
    public CriarSessaoValidator()
    {
        RuleFor(r => r.UsuarioEmail).NotEmpty().WithMessage(MensagensValidacao.Obrigatorio).EmailAddress().WithMessage(MensagensValidacao.EmailInvalido).MaximumLength(256).WithMessage(MensagensValidacao.TamanhoMaximo);
        RuleFor(r => r.Senha).NotEmpty().WithMessage(MensagensValidacao.Obrigatorio).MaximumLength(128).WithMessage(MensagensValidacao.TamanhoMaximo);
    }
}
