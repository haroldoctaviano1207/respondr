namespace Respondr.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Respondr.Domain.Incidents;
using Respondr.Shared.Constants;

public sealed class IncidentConfiguration : IEntityTypeConfiguration<Incident>
{
    public void Configure(EntityTypeBuilder<Incident> builder)
    {
        builder.ToTable("Incidents", SchemaNames.Incidents);
        builder.HasKey(x => x.Id);
        builder.Property(x => x.IncidentNumber).HasMaxLength(40).IsRequired();
        builder.HasIndex(x => x.IncidentNumber).IsUnique();
        builder.Property(x => x.Title).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Type).HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(x => x.Priority).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(x => x.IncidentLocationId).IsRequired();
        builder.Property(x => x.SituationSummary).HasMaxLength(2000).IsRequired();
        builder.Property(x => x.ReportedAt).IsRequired();
        builder.HasOne(x => x.Location)
            .WithOne()
            .HasForeignKey<Incident>(x => x.IncidentLocationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
