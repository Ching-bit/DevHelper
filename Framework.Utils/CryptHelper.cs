using System.Security.Cryptography;

namespace Framework.Utils;

public static class CryptHelper
{
    public static string Md5File(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return string.Empty;
        }

        using MD5 md5 = MD5.Create();
        using Stream stream = File.OpenRead(filePath);
        byte[] hash = md5.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}