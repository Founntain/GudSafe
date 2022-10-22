using GudSafe.Data.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GudSafe.Data.Configurations;

public class GudCollectionConfiguration : BaseConfiguration<GudCollection>
{
    public override void Configure(EntityTypeBuilder<GudCollection> builder)
    {
        base.Configure(builder);

        builder.HasIndex(x => x.Name).IsUnique();

        builder.HasMany(x => x.Files).WithMany(x => x.Collections);
    }
}