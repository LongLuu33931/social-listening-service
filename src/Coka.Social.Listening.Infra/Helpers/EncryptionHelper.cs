using System.Security.Cryptography;
using System.Text;

namespace Coka.Social.Listening.Infra.Helpers;

public static class EncryptionHelper
{
    // AES-256 encryption key (32 bytes) — derived from a stable passphrase
    private static readonly byte[] Key = SHA256.HashData(Encoding.UTF8.GetBytes("Coka.Social.Listening.SecretKey.2024!@#"));

    // Fixed IV (16 bytes) for deterministic encryption — suitable for config values
    private static readonly byte[] IV = MD5.HashData(Encoding.UTF8.GetBytes("Coka.Listening.IV"));

    public static string Encrypt(string plainText)
    {
        using var aes = Aes.Create();
        aes.Key = Key;
        aes.IV = IV;

        using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream();
        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
        using (var sw = new StreamWriter(cs))
        {
            sw.Write(plainText);
        }

        return Convert.ToBase64String(ms.ToArray());
    }

    public static string Decrypt(string cipherText)
    {
        var buffer = Convert.FromBase64String(cipherText);

        using var aes = Aes.Create();
        aes.Key = Key;
        aes.IV = IV;

        using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream(buffer);
        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var sr = new StreamReader(cs);

        return sr.ReadToEnd();
    }
}
