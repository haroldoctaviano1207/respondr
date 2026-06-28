namespace Respondr.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Respondr.Domain.Resources;
using Respondr.Shared.Constants;

public sealed class ResourceItemConfiguration : IEntityTypeConfiguration<ResourceItem>
{
    public void Configure(EntityTypeBuilder<ResourceItem> builder)
    {
        builder.ToTable("ResourceItems", SchemaNames.Resources);
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Type).HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(x => x.IsAvailable).HasDefaultValue(true);
    }
}
