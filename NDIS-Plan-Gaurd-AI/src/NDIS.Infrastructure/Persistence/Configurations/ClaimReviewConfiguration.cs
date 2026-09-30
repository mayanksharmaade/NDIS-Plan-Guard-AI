using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NDIS.Domain.Entities;

namespace NDIS.Infrastructure.Persistence.Configurations;

public sealed class ClaimReviewConfiguration : IEntityTypeConfiguration<ClaimReview>
{
    public void Configure(EntityTypeBuilder<ClaimReview> builder)
    {
        builder.ToTable("ClaimReviews");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ReviewerEmail).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Outcome).HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(x => x.Comments).HasMaxLength(1000);
        builder.HasIndex(x => new { x.ClaimId, x.ReviewedAtUtc });
        builder.HasIndex(x => x.ReviewerIdentityUserId);
        builder.HasOne(x => x.Claim)
            .WithMany()
            .HasForeignKey(x => x.ClaimId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
