using System.Security.Cryptography;

namespace Backend.Constants;

public static class Cryptography
{
    public static readonly int SaltSize = 16;
    public static readonly int HashSize = 32;
    public static readonly int Iterations = 100000;
    public static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;
}