using System.Security.Cryptography;
using System.Text;

namespace ConverterAPI.Services;

public static class PasswordManager
{
    public static (string hash, string salt) HashPassword(string? password)
    {
        if (password == null)
        {
            throw new ArgumentNullException("password");
        }
        
        // Generate random salt
        byte[] saltBytes = RandomNumberGenerator.GetBytes(16); // 16 bytes of salt
        string salt = Convert.ToBase64String(saltBytes);

        // Hash password with salt
        using var hmac = new HMACSHA256(saltBytes);
        byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
        byte[] hashBytes = hmac.ComputeHash(passwordBytes);
        string hash = Convert.ToBase64String(hashBytes);

        return (hash, salt);
    }
    
    public static bool VerifyPassword(string? password, string? storedHash, string? storedSalt)
    {
        if (storedHash == null || storedSalt == null)
        {
            return false;
        }
        ArgumentNullException.ThrowIfNull(password);

        // Convert salt from DB to bytes
        byte[] saltBytes = Convert.FromBase64String(storedSalt);

        // Hash entered password with salt from DB
        using var hmac = new HMACSHA256(saltBytes);
        byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
        byte[] computedHashBytes = hmac.ComputeHash(passwordBytes);
        string computedHash = Convert.ToBase64String(computedHashBytes);

        // Compare passwords
        return computedHash == storedHash;
    }
}