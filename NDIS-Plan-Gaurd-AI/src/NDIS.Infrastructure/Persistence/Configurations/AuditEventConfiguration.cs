using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NDIS.Domain.Entities;

namespace NDIS.Infrastructure.Persistence.Configurations;

public sealed class AuditEventConfiguration : IEntityTypeConfiguration<AuditEvent>
{
    public void Configure(EntityTypeBuilder<AuditEvent> builder)
    {
        builder.ToTable("AuditEvents");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.EntityType).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Action).HasConversion<string>().HasMaxLength(100).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.ActorEmail).HasMaxLength(256);
        builder.HasIndex(x => new { x.ServiceProviderId, x.OccurredAtUtc });
        builder.HasIndex(x => new { x.EntityType, x.EntityId, x.OccurredAtUtc });
        builder.HasIndex(x => x.ActorIdentityUserId);
    }
}
