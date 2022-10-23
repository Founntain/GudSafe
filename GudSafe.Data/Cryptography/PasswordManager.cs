using System.Security.Cryptography;
using System.Text;

namespace GudSafe.Data.Cryptography;

public class PasswordManager
{
    /// <summary>
    /// Hashes and salts a given password
    /// </summary>
    /// <param name="password">The password to be hashed</param>
    /// <param name="salt">The salt generated</param>
    /// <param name="hashedPassword">The hashed password</param>
    public static void HashPassword(string password, out string salt, out string hashedPassword)
    {
        salt = HashString(GetSalt());

        var passwordWithSalt = password + salt;

        hashedPassword = HashString(passwordWithSalt);
    }

    public static bool CheckIfPasswordIsCorrect(string password, string salt, string storedPassword)
    {
        var passwordWithSalt = password + salt;

        var hashedPassword = HashString(passwordWithSalt);

        var result = hashedPassword == storedPassword;

        return result;
    }

    private static string HashString(string input)
    {
        using (var sha256Hash = SHA256.Create())
        {
            var data = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            var builder = new StringBuilder();

            for (var i = 0; i < data.Length; i++)
            {
                builder.Append(data[i].ToString("x2"));
            }

            var hash = builder.ToString();

            return hash;
        }
    }

    private static string GetSalt()
    {
        var rng = new RNGCryptoServiceProvider();

        var data = new byte[2048];
        rng.GetBytes(data);

        var salt = BitConverter.ToString(data);

        return salt;
    }
}