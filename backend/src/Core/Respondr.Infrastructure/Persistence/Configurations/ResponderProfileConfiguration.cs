namespace Respondr.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Respondr.Domain.Dispatch;
using Respondr.Shared.Constants;

public sealed class ResponderProfileConfiguration : IEntityTypeConfiguration<ResponderProfile>
{
    public void Configure(EntityTypeBuilder<ResponderProfile> builder)
    {
        builder.ToTable("ResponderProfiles", SchemaNames.Dispatch);
        builder.HasKey(x => x.Id);
        builder.Property(x => x.DisplayName).HasMaxLength(150).IsRequired();
        builder.Property(x => x.ResponderType).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.HasIndex(x => x.UserId).IsUnique();
    }
}
