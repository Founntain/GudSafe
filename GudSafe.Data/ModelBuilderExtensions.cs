using GudSafe.Data.Cryptography;
using GudSafe.Data.Entities;
using GudSafe.Data.Enums;
using Microsoft.EntityFrameworkCore;

namespace GudSafe.Data;

public static class ModelBuilderExtensions
{
    public static void Seed(this ModelBuilder builder)
    {
        PasswordManager.HashPassword("admin", out var salt, out var password);

        builder.Entity<User>().HasData(new List<User>
        {
            new ()
            {
                ID = 1,
                Name = "admin",
                UserRole = UserRole.Admin,
                Password = password,
                Salt = salt
            }
        });
    }
}