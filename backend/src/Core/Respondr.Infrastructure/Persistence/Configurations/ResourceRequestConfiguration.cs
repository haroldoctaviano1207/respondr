namespace Respondr.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Respondr.Domain.Resources;
using Respondr.Shared.Constants;

public sealed class ResourceRequestConfiguration : IEntityTypeConfiguration<ResourceRequest>
{
    public void Configure(EntityTypeBuilder<ResourceRequest> builder)
    {
        builder.ToTable("ResourceRequests", SchemaNames.Resources);
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ResourceType).HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(x => x.Quantity).IsRequired();
        builder.Property(x => x.Justification).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(30).IsRequired();
    }
}
