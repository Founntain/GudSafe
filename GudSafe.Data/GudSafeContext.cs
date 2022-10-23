using GudSafe.Data.Configurations;
using GudSafe.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace GudSafe.Data;

public class GudSafeContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<GudFile> Files { get; set; }
    public DbSet<GudCollection> Collections { get; set; }

    public GudSafeContext()
    {
        Database.EnsureCreated();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new GudFileConfiguration());
        modelBuilder.ApplyConfiguration(new GudCollectionConfiguration());

        modelBuilder.Seed();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=GudSafe.db");
        optionsBuilder.UseLazyLoadingProxies();
    }
}