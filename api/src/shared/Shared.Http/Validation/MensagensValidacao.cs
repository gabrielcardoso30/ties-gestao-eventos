namespace Shared.Http.Validation;

/// <summary>Mensagens padrão em pt-BR reutilizadas pelos validators dos módulos.</summary>
public static class MensagensValidacao
{
    public const string Obrigatorio = "O campo {PropertyName} é obrigatório.";
    public const string TamanhoMaximo = "O campo {PropertyName} deve ter no máximo {MaxLength} caracteres.";
    public const string EmailInvalido = "O campo {PropertyName} deve ser um e-mail válido.";
    public const string MaiorQueZero = "O campo {PropertyName} deve ser maior que zero.";
    public const string GuidObrigatorio = "O campo {PropertyName} deve ser um identificador válido.";
}
