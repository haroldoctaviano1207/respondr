namespace Respondr.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Respondr.Domain.Dispatch;
using Respondr.Shared.Constants;

public sealed class DispatchAssignmentConfiguration : IEntityTypeConfiguration<DispatchAssignment>
{
    public void Configure(EntityTypeBuilder<DispatchAssignment> builder)
    {
        builder.ToTable("DispatchAssignments", SchemaNames.Dispatch);
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(x => x.AssignedAt).IsRequired();
        builder.HasIndex(x => new { x.IncidentId, x.ResponderProfileId }).IsUnique();
    }
}
