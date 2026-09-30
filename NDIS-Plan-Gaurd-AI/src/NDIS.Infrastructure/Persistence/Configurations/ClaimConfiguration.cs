using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NDIS.Domain.Entities;

namespace NDIS.Infrastructure.Persistence.Configurations;

public sealed class ClaimConfiguration : IEntityTypeConfiguration<Claim>
{
    public void Configure(EntityTypeBuilder<Claim> builder)
    {
        builder.ToTable("Claims");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ClaimNumber).HasMaxLength(50).IsRequired();
        builder.Property(x => x.ServiceFrom).HasColumnType("date");
        builder.Property(x => x.ServiceTo).HasColumnType("date");
        builder.Property(x => x.SupportCategory).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.Amount).HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.Units);
        builder.Property(x => x.UnitPrice).HasPrecision(18, 2);
        builder.Property(x => x.TotalServiceHours).HasPrecision(18, 2);
        builder.Property(x => x.AgreedHourlyRate).HasPrecision(18, 2);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.HasIndex(x => new { x.ServiceProviderId, x.ClaimNumber }).IsUnique();
        builder.HasIndex(x => x.ParticipantId);
        builder.HasIndex(x => new { x.Status, x.SubmittedAtUtc });
        builder.HasOne(x => x.ServiceProvider).WithMany().HasForeignKey(x => x.ServiceProviderId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Participant).WithMany().HasForeignKey(x => x.ParticipantId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.ServiceDelivery)
    .WithMany()
    .HasForeignKey(x => x.ServiceDeliveryId)
    .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ServiceProviderEmployee)
            .WithMany()
            .HasForeignKey(x => x.ServiceProviderEmployeeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
