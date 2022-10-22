using GudSafe.Data.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GudSafe.Data.Configurations;

public class GudFileConfiguration : BaseConfiguration<GudFile>
{
    public override void Configure(EntityTypeBuilder<GudFile> builder)
    {
        base.Configure(builder);

        builder.HasIndex(x => x.Name).IsUnique();

        builder.HasOne(x => x.Creator).WithMany(x => x.FilesUploaded);
        builder.HasMany(x => x.Collections).WithMany(x => x.Files);
    }
}