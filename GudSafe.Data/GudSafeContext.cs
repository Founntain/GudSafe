using GudSafe.Data.Configurations;
using GudSafe.Data.Cryptography;
using GudSafe.Data.Entities;
using GudSafe.Data.Enums;
using Microsoft.EntityFrameworkCore;

namespace GudSafe.Data;

public sealed class GudSafeContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<GudFile> Files { get; set; }
    public DbSet<GudCollection> Collections { get; set; }

    public GudSafeContext()
    {
        try
        {
            if (Users?.Any() ?? true) return;

            PasswordManager.HashPassword("admin", out var salt, out var password);

            Users.Add(new User
            {
                ID = 1,
                Name = "admin",
                UserRole = UserRole.Admin,
                Password = password,
                Salt = salt
            });

            SaveChanges();
        }
        catch (Exception e)
        {
            // used only for migration because initially user table does not exist
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new GudFileConfiguration());
        modelBuilder.ApplyConfiguration(new GudCollectionConfiguration());

        //modelBuilder.Seed();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=GudSafe.db");
        optionsBuilder.UseLazyLoadingProxies();
    }
}