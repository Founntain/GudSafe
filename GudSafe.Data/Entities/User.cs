using System.Security.Cryptography;

namespace GudSafe.Data.Entities;

public class User : BaseEntity
{
    public string Password { get; set; }
    public string Salt { get; set; }
    public string ApiKey { get; set; } = GenerateApiKey();
    public long LastChangedTicks { get; set; } = DateTime.Now.Ticks;

    public virtual HashSet<GudFile> FilesUploaded { get; set; } = new();

    private static string GenerateApiKey()
    {
        var key = new byte[64];

        using (var generator = RandomNumberGenerator.Create())
            generator.GetBytes(key);

        return Convert.ToBase64String(key);
    }
}