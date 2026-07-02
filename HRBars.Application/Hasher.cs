using System.Security.Cryptography;
using System.Text;

namespace HRBars.Application;

public static class Hasher
{
    public static bool VerifyPasswordHash(string password, string storedHash)
    {
        try
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            var computedHash = Convert.ToHexString(hash).ToLower();
            
            return computedHash == storedHash;
        }
        catch
        {
            return false;
        }
    }
}