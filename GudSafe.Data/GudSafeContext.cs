using GudSafe.Data.Configurations;
using GudSafe.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;

namespace GudSafe.Data;

public class GudSafeContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<GudFile> Files { get; set; }
    public DbSet<Collection> Collections { get; set; }

    public GudSafeContext()
    {
        Database.EnsureCreated();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new GudFileConfiguration());
        modelBuilder.ApplyConfiguration(new GudCollectionConfiguration());
    }
}