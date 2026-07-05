using System.Security.Cryptography;
using System.Text;

namespace HRBars.Application;

public static class Hasher
{
    public static bool VerifyPasswordHash(string password, string storedHash)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(storedHash))
            return false;

        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        var computedHash = Convert.ToBase64String(hashedBytes);

        return computedHash == storedHash;
    }
}