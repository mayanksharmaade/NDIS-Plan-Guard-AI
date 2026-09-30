using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NDIS.Domain.Entities;
using NDIS.Infrastructure.Identity;
namespace NDIS.Infrastructure.Persistence.Configurations;

public class UserProfileConfiguration
    : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(
        EntityTypeBuilder<UserProfile> builder)
    {
        builder.ToTable("UserProfiles");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.FirstName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.LastName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.IdentityUserId)
            .IsRequired();

        builder.HasIndex(x => x.IdentityUserId)
            .IsUnique();

        builder.Property(x => x.UserCategory)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.ApprovalStatus)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.AccountStatus)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();
        builder.HasOne<ApplicationUser>()
    .WithOne()
    .HasForeignKey<UserProfile>(x => x.IdentityUserId)
    .OnDelete(DeleteBehavior.Restrict);
    }
}