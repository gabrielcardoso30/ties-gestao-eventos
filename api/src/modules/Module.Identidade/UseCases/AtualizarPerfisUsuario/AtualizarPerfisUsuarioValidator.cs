using FluentValidation;
using Shared.Http.Validation;

namespace Module.Identidade.UseCases.AtualizarPerfisUsuario;

internal sealed class AtualizarPerfisUsuarioValidator : AbstractValidator<AtualizarPerfisUsuarioRequest>
{
    public AtualizarPerfisUsuarioValidator()
    {
        RuleFor(r => r.UsuarioId).NotEmpty().WithMessage(MensagensValidacao.GuidObrigatorio);
        RuleFor(r => r.Perfis).NotNull().WithMessage(MensagensValidacao.Obrigatorio);
        RuleForEach(r => r.Perfis).NotEmpty().WithMessage("Perfil não pode ser vazio.").MaximumLength(50).WithMessage(MensagensValidacao.TamanhoMaximo);
    }
}
