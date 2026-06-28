namespace Respondr.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Respondr.Domain.Incidents;
using Respondr.Shared.Constants;

public sealed class IncidentLocationConfiguration : IEntityTypeConfiguration<IncidentLocation>
{
    public void Configure(EntityTypeBuilder<IncidentLocation> builder)
    {
        builder.ToTable("IncidentLocations", SchemaNames.Incidents);
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Address).HasMaxLength(200).IsRequired();
        builder.Property(x => x.City).HasMaxLength(100);
        builder.Property(x => x.Region).HasMaxLength(100);
    }
}
