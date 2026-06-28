namespace Respondr.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Respondr.Domain.Incidents;
using Respondr.Shared.Constants;

public sealed class IncidentHistoryConfiguration : IEntityTypeConfiguration<IncidentHistory>
{
    public void Configure(EntityTypeBuilder<IncidentHistory> builder)
    {
        builder.ToTable("IncidentHistories", SchemaNames.Incidents);
        builder.HasKey(x => x.Id);
        builder.Property(x => x.EventType).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500).IsRequired();
    }
}
