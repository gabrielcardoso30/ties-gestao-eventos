using FluentValidation;
using Shared.Contracts.Common;

namespace Module.Locais.UseCases.ListarLocais;

internal sealed class ListarLocaisValidator : AbstractValidator<ListarLocaisRequest>
{
    public ListarLocaisValidator()
    {
        RuleFor(r => r.Pagina).GreaterThanOrEqualTo(1);
        RuleFor(r => r.TamanhoPagina).InclusiveBetween(1, PagedRequest.TamanhoMaximo);
        RuleFor(r => r.EnderecoUf).Length(2).When(r => !string.IsNullOrWhiteSpace(r.EnderecoUf));
        RuleFor(r => r.Busca).MaximumLength(100);
        RuleFor(r => r.OrdenarPor).Must(c => new[] { "localNome", "enderecoCidade", "salasQuantidade", "localCapacidadeTotal" }.Contains(c!, StringComparer.OrdinalIgnoreCase)).When(r => !string.IsNullOrWhiteSpace(r.OrdenarPor));
    }
}
