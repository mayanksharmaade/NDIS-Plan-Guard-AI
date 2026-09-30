using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NDIS.Domain.Entities;

namespace NDIS.Infrastructure.Persistence.Configurations;

public sealed class ClaimRiskAssessmentConfiguration : IEntityTypeConfiguration<ClaimRiskAssessment>
{
    public void Configure(EntityTypeBuilder<ClaimRiskAssessment> builder)
    {
        builder.ToTable("ClaimRiskAssessments");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Probability).HasPrecision(9, 6);
        builder.Property(x => x.RiskBand).HasMaxLength(30);
        builder.Property(x => x.ModelVersion).HasMaxLength(100);
        builder.Property(x => x.FeatureVersion).HasMaxLength(100);
        builder.Property(x => x.FailureReason).HasMaxLength(1000);
        builder.HasIndex(x => new { x.ClaimId, x.CreatedAtUtc });

        builder.HasOne(x => x.Claim)
            .WithMany()
            .HasForeignKey(x => x.ClaimId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Factors)
            .WithOne(x => x.ClaimRiskAssessment)
            .HasForeignKey(x => x.ClaimRiskAssessmentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
