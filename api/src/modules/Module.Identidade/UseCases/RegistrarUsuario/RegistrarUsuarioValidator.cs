using FluentValidation;
using Shared.Http.Validation;

namespace Module.Identidade.UseCases.RegistrarUsuario;

internal sealed class RegistrarUsuarioValidator : AbstractValidator<RegistrarUsuarioRequest>
{
    public RegistrarUsuarioValidator()
    {
        RuleFor(r => r.UsuarioNome).NotEmpty().WithMessage(MensagensValidacao.Obrigatorio).MaximumLength(150).WithMessage(MensagensValidacao.TamanhoMaximo);
        RuleFor(r => r.UsuarioEmail).NotEmpty().WithMessage(MensagensValidacao.Obrigatorio).EmailAddress().WithMessage(MensagensValidacao.EmailInvalido).MaximumLength(256).WithMessage(MensagensValidacao.TamanhoMaximo);
        // A força da senha é avaliada pelo Identity (422 Identidade.SenhaFraca); aqui só o básico.
        RuleFor(r => r.Senha).NotEmpty().WithMessage(MensagensValidacao.Obrigatorio).MaximumLength(128).WithMessage(MensagensValidacao.TamanhoMaximo);
        RuleFor(r => r.Perfis).NotNull().WithMessage(MensagensValidacao.Obrigatorio).NotEmpty().WithMessage("Informe ao menos um perfil.");
        RuleForEach(r => r.Perfis).NotEmpty().WithMessage("Perfil não pode ser vazio.").MaximumLength(50).WithMessage(MensagensValidacao.TamanhoMaximo);
    }
}
