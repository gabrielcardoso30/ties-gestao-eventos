using FluentValidation;
using Shared.Http.Validation;

namespace Module.Identidade.UseCases.ObterUsuarioAtual;

internal sealed class ObterUsuarioAtualValidator : AbstractValidator<ObterUsuarioAtualRequest>
{
    public ObterUsuarioAtualValidator() => RuleFor(r => r.UsuarioId).NotEmpty().WithMessage(MensagensValidacao.GuidObrigatorio);
}
