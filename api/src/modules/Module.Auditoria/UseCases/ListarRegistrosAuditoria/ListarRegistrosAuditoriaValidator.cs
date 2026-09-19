using FluentValidation;
using Shared.Contracts.Auditoria;
using Shared.Contracts.Common;
using Shared.Http.Validation;

namespace Module.Auditoria.UseCases.ListarRegistrosAuditoria;

internal sealed class ListarRegistrosAuditoriaValidator : AbstractValidator<ListarRegistrosAuditoriaRequest>
{
    private static readonly string[] Operacoes = [OperacoesAuditoria.Inclusao, OperacoesAuditoria.Alteracao, OperacoesAuditoria.Exclusao];

    public ListarRegistrosAuditoriaValidator()
    {
        RuleFor(r => r.Pagina).GreaterThanOrEqualTo(1);
        RuleFor(r => r.TamanhoPagina).InclusiveBetween(1, PagedRequest.TamanhoMaximo);
        RuleFor(r => r.Modulo).MaximumLength(50).WithMessage(MensagensValidacao.TamanhoMaximo);
        RuleFor(r => r.EntidadeNome).MaximumLength(100).WithMessage(MensagensValidacao.TamanhoMaximo);
        RuleFor(r => r.EntidadeId).MaximumLength(100).WithMessage(MensagensValidacao.TamanhoMaximo);
        RuleFor(r => r.Operacao)
            .Must(o => Operacoes.Contains(o!.Trim(), StringComparer.OrdinalIgnoreCase))
            .When(r => !string.IsNullOrWhiteSpace(r.Operacao))
            .WithMessage($"O campo {{PropertyName}} deve ser um de: {string.Join(", ", Operacoes)}.");
        RuleFor(r => r.OcorridoAte)
            .GreaterThanOrEqualTo(r => r.OcorridoDe)
            .When(r => r.OcorridoDe.HasValue && r.OcorridoAte.HasValue)
            .WithMessage("O campo {PropertyName} deve ser maior ou igual a OcorridoDe.");
    }
}
