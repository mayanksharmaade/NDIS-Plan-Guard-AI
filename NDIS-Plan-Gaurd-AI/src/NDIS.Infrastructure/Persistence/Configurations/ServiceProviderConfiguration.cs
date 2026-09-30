using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NDIS.Domain.Entities;

namespace NDIS.Infrastructure.Persistence.Configurations;

public class ServiceProviderConfiguration
    : IEntityTypeConfiguration<ServiceProvider>
{
    public void Configure(
        EntityTypeBuilder<ServiceProvider> builder)
    {
        builder.ToTable("ServiceProviders");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.LegalName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.TradingName)
            .HasMaxLength(200);

        builder.Property(x => x.Abn)
            .HasMaxLength(11)
            .IsRequired();

        builder.HasIndex(x => x.Abn)
            .IsUnique();

        builder.Property(x => x.NdisRegistrationNumber)
            .HasMaxLength(100);

        builder.Property(x => x.ApprovalStatus)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();
    }
}