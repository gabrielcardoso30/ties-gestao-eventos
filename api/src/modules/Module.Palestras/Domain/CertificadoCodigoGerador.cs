using System.Security.Cryptography;

namespace Module.Palestras.Domain;

/// <summary>
/// Gera códigos de certificado com 12 caracteres de um alfabeto sem símbolos ambíguos (sem 0/O, 1/I/L),
/// usando gerador criptográfico. O alfabeto possui 31 símbolos; <see cref="RandomNumberGenerator.GetInt32(int)"/>
/// faz amostragem sem viés mesmo quando a quantidade não é potência de dois.
/// </summary>
public static class CertificadoCodigoGerador
{
    public const string Alfabeto = "ABCDEFGHJKMNPQRSTUVWXYZ23456789";
    public const int Tamanho = 12;

    public static string Gerar()
    {
        Span<char> codigo = stackalloc char[Tamanho];
        for (var i = 0; i < Tamanho; i++)
        {
            codigo[i] = Alfabeto[RandomNumberGenerator.GetInt32(Alfabeto.Length)];
        }

        return new string(codigo);
    }
}
