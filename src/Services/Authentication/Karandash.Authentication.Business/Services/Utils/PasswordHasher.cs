using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace Karandash.Authentication.Business.Services.Utils;

public class PasswordHasher
{
    public string Hash(string password)
    {
        return Convert.ToBase64String(
            KeyDerivation.Pbkdf2(
                password: password,
                salt: GenerateSalt(),
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 32
            )
        );
    }

    /* Girilən parol'u check etməyimiz üçün gərəklidir! */
    public bool Verify(string password, string passwordHash)
    {
        return Hash(password) == passwordHash;
    }

    private byte[] GenerateSalt()
    {
        byte[] salt = new byte[16];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(salt);
        return salt;
    }
}