using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NDIS.Domain.Entities;

namespace NDIS.Infrastructure.Persistence.Configurations;

public class ServiceProviderUserConfiguration
    : IEntityTypeConfiguration<ServiceProviderUser>
{
    public void Configure(
        EntityTypeBuilder<ServiceProviderUser> builder)
    {
        builder.ToTable("ServiceProviderUsers");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(x => new
        {
            x.ServiceProviderId,
            x.UserProfileId
        })
        .IsUnique();

        builder.HasOne(x => x.ServiceProvider)
            .WithMany(x => x.Users)
            .HasForeignKey(x => x.ServiceProviderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.UserProfile)
            .WithMany(x => x.ServiceProviderMemberships)
            .HasForeignKey(x => x.UserProfileId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}