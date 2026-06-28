namespace Respondr.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Respondr.Domain.Resources;
using Respondr.Shared.Constants;

public sealed class ResourceAllocationConfiguration : IEntityTypeConfiguration<ResourceAllocation>
{
    public void Configure(EntityTypeBuilder<ResourceAllocation> builder)
    {
        builder.ToTable("ResourceAllocations", SchemaNames.Resources);
        builder.HasKey(x => x.Id);
        builder.Property(x => x.AllocatedAt).IsRequired();
    }
}
