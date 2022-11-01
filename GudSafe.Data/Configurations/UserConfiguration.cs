using GudSafe.Data.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GudSafe.Data.Configurations;

public class UserConfiguration : BaseConfiguration<User>
{
    public override void Configure(EntityTypeBuilder<User> builder)
    {
        base.Configure(builder);

        builder.HasIndex(x => x.Name).IsUnique();
        builder.HasIndex(x => x.ApiKey).IsUnique();

        builder.HasMany(x => x.FilesUploaded).WithOne(x => x.Creator);
    }
}