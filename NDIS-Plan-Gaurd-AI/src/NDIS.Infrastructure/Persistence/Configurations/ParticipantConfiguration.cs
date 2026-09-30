using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NDIS.Domain.Entities;

namespace NDIS.Infrastructure.Persistence.Configurations;

public sealed class ParticipantConfiguration : IEntityTypeConfiguration<Participant>
{
    public void Configure(EntityTypeBuilder<Participant> builder)
    {
        builder.ToTable("Participants");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.NdisNumber).HasMaxLength(50).IsRequired();
        builder.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.LastName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Email).HasMaxLength(256);
        builder.Property(x => x.PhoneNumber).HasMaxLength(30);
        builder.Property(x => x.DateOfBirth).HasColumnType("date");
        builder.Property(x => x.PlanStartDate).HasColumnType("date");
        builder.Property(x => x.PlanEndDate).HasColumnType("date");
        builder.Property(x => x.EmergencyContactName)
    .HasMaxLength(150);

        builder.Property(x => x.EmergencyContactRelationship)
            .HasMaxLength(100);

        builder.Property(x => x.EmergencyContactPhoneNumber)
            .HasMaxLength(20);
        builder.Property(x => x.PlanTotalBudget).HasPrecision(18, 2);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.HasIndex(x => new { x.ServiceProviderId, x.NdisNumber }).IsUnique();
        builder.HasOne(x => x.ServiceProvider).WithMany().HasForeignKey(x => x.ServiceProviderId).OnDelete(DeleteBehavior.Restrict);
    }
}
