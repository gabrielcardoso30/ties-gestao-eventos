using System.Security.Cryptography;

namespace Module.Palestras.Domain;

/// <summary>
/// Gera códigos de certificado com 12 caracteres de um alfabeto sem símbolos ambíguos (sem 0/O, 1/I/L),
/// usando gerador criptográfico. O alfabeto tem 32 símbolos, o que permite mapear cada byte sem viés (5 bits).
/// </summary>
public static class CertificadoCodigoGerador
{
    public const string Alfabeto = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
    public const int Tamanho = 12;

    public static string Gerar()
    {
        Span<byte> aleatorios = stackalloc byte[Tamanho];
        RandomNumberGenerator.Fill(aleatorios);

        Span<char> codigo = stackalloc char[Tamanho];
        for (var i = 0; i < Tamanho; i++)
        {
            codigo[i] = Alfabeto[aleatorios[i] & (Alfabeto.Length - 1)];
        }

        return new string(codigo);
    }
}
