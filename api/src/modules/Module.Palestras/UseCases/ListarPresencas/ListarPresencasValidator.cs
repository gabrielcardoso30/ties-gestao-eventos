using FluentValidation;
using Shared.Http.Validation;

namespace Module.Palestras.UseCases.ListarPresencas;

internal sealed class ListarPresencasValidator : AbstractValidator<ListarPresencasRequest>
{
    public ListarPresencasValidator() => RuleFor(r => r.PalestraId).NotEmpty().WithMessage(MensagensValidacao.GuidObrigatorio);
}
