using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NDIS.Domain.Entities;

namespace NDIS.Infrastructure.Persistence.Configurations;

public sealed class ClaimRiskFactorConfiguration : IEntityTypeConfiguration<ClaimRiskFactor>
{
    public void Configure(EntityTypeBuilder<ClaimRiskFactor> builder)
    {
        builder.ToTable("ClaimRiskFactors");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Feature).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.ObservedValueJson).HasMaxLength(2000);
        builder.Property(x => x.BaselineValueJson).HasMaxLength(2000);
        builder.HasIndex(x => x.ClaimRiskAssessmentId);
    }
}
