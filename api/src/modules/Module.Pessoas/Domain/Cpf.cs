namespace Module.Pessoas.Domain;

/// <summary>
/// Regras do CPF (Cadastro de Pessoas Físicas): normalização (somente dígitos) e validação dos dígitos verificadores.
/// O documento é armazenado sempre normalizado; a máscara é responsabilidade da apresentação.
/// </summary>
public static class Cpf
{
    public const int Tamanho = 11;

    /// <summary>Remove máscara e espaços, mantendo apenas dígitos. Retorna <c>null</c> quando não restar nenhum dígito.</summary>
    public static string? Normalizar(string? documento)
    {
        if (string.IsNullOrWhiteSpace(documento))
        {
            return null;
        }

        var digitos = new string(documento.Where(char.IsAsciiDigit).ToArray());
        return digitos.Length == 0 ? null : digitos;
    }

    /// <summary>Valida tamanho, sequência repetida (ex.: 111.111.111-11) e os dois dígitos verificadores (módulo 11).</summary>
    public static bool EhValido(string? documento)
    {
        var digitos = Normalizar(documento);
        if (digitos is null || digitos.Length != Tamanho || digitos.Distinct().Count() == 1)
        {
            return false;
        }

        return CalcularDigito(digitos, 9) == digitos[9] - '0' && CalcularDigito(digitos, 10) == digitos[10] - '0';
    }

    private static int CalcularDigito(string digitos, int quantidade)
    {
        var soma = 0;
        var peso = quantidade + 1;
        for (var i = 0; i < quantidade; i++)
        {
            soma += (digitos[i] - '0') * peso--;
        }

        var resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }
}
