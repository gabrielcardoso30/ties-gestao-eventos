using Shared.Http.Results;

namespace Module.Locais.Domain;

public static class LocaisErros
{
    public static readonly Error LocalNaoEncontrado = Error.NotFound("Locais.LocalNaoEncontrado", "Local não encontrado.");
    public static readonly Error LocalNomeDuplicado = Error.Conflict("Locais.LocalNomeDuplicado", "Já existe um local com este nome.");
    public static readonly Error SalaNaoEncontrada = Error.NotFound("Locais.SalaNaoEncontrada", "Sala não encontrada neste local.");
    public static readonly Error SalaNomeDuplicado = Error.Conflict("Locais.SalaNomeDuplicado", "Já existe uma sala com este nome neste local.");
    public static readonly Error LocalPrecisaDeUmaSala = Error.BusinessRule("Locais.LocalPrecisaDeUmaSala", "Um local precisa manter ao menos uma sala (ambiente).");
}
