using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NDIS.Domain.Entities;

namespace NDIS.Infrastructure.Persistence.Configurations;

public sealed class ServiceProviderEmployeeConfiguration
    : IEntityTypeConfiguration<ServiceProviderEmployee>
{
    public void Configure(
        EntityTypeBuilder<ServiceProviderEmployee> builder)
    {
        builder.ToTable("ServiceProviderEmployees");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.EmployeeNumber)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.FirstName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.LastName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Email)
            .HasMaxLength(250);

        builder.Property(x => x.DefaultHourlyRate)
            .HasPrecision(18, 2);

        builder.Property(x => x.Role)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.HasIndex(x => new
        {
            x.ServiceProviderId,
            x.EmployeeNumber
        })
        .IsUnique();

        builder.HasOne(x => x.ServiceProvider)
            .WithMany()
            .HasForeignKey(x => x.ServiceProviderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}


public sealed class ParticipantServiceAssignmentConfiguration
    : IEntityTypeConfiguration<ParticipantServiceAssignment>
{
    public void Configure(
        EntityTypeBuilder<ParticipantServiceAssignment> builder)
    {
        builder.ToTable("ParticipantServiceAssignments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.SupportCategory)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.AgreedHourlyRate)
            .HasPrecision(18, 2);

        builder.HasOne(x => x.Participant)
            .WithMany()
            .HasForeignKey(x => x.ParticipantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ServiceProviderEmployee)
            .WithMany(x => x.ParticipantAssignments)
            .HasForeignKey(x => x.ServiceProviderEmployeeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}


public sealed class ServiceDeliveryConfiguration
    : IEntityTypeConfiguration<ServiceDelivery>
{
    public void Configure(
        EntityTypeBuilder<ServiceDelivery> builder)
    {
        builder.ToTable("ServiceDeliveries");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.SupportCategory)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.ServiceHours)
            .HasPrecision(18, 2);

        builder.Property(x => x.HourlyRate)
            .HasPrecision(18, 2);

        builder.Property(x => x.Amount)
            .HasPrecision(18, 2);

        builder.Property(x => x.ServiceLocation)
            .HasMaxLength(300);

        builder.HasOne(x => x.Participant)
            .WithMany()
            .HasForeignKey(x => x.ParticipantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ServiceProviderEmployee)
            .WithMany(x => x.ServiceDeliveries)
            .HasForeignKey(x => x.ServiceProviderEmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ParticipantServiceAssignment)
            .WithMany()
            .HasForeignKey(x => x.ParticipantServiceAssignmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new
        {
            x.ServiceProviderEmployeeId,
            x.ServiceStartUtc,
            x.ServiceEndUtc
        });
    }
}


public sealed class ParticipantBudgetPlanConfiguration
    : IEntityTypeConfiguration<ParticipantBudgetPlan>
{
    public void Configure(
        EntityTypeBuilder<ParticipantBudgetPlan> builder)
    {
        builder.ToTable("ParticipantBudgetPlans");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.PlanName)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.TotalBudget)
            .HasPrecision(18, 2);

        builder.Property(x => x.DefaultFortnightBudget)
            .HasPrecision(18, 2);

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.HasOne(x => x.Participant)
            .WithMany()
            .HasForeignKey(x => x.ParticipantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.BudgetPlanTemplate)
            .WithMany(x => x.ParticipantPlans)
            .HasForeignKey(x => x.BudgetPlanTemplateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Allocations)
            .WithOne(x => x.ParticipantBudgetPlan)
            .HasForeignKey(x => x.ParticipantBudgetPlanId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}


public sealed class ParticipantBudgetAllocationConfiguration
    : IEntityTypeConfiguration<ParticipantBudgetAllocation>
{
    public void Configure(
        EntityTypeBuilder<ParticipantBudgetAllocation> builder)
    {
        builder.ToTable("ParticipantBudgetAllocations");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.SupportCategory)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.AllocatedAmount)
            .HasPrecision(18, 2);

        builder.Property(x => x.FortnightLimit)
            .HasPrecision(18, 2);

        builder.HasIndex(x => new
        {
            x.ParticipantBudgetPlanId,
            x.SupportCategory
        })
        .IsUnique();
    }
}
public sealed class BudgetPlanTemplateConfiguration
    : IEntityTypeConfiguration<BudgetPlanTemplate>
{
    public void Configure(EntityTypeBuilder<BudgetPlanTemplate> builder)
    {
        builder.ToTable("BudgetPlanTemplates");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.PlanName).HasMaxLength(150).IsRequired();
        builder.Property(x => x.BudgetType).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(x => x.Amount).HasPrecision(18, 2).IsRequired();
        builder.HasIndex(x => x.PlanName).IsUnique();
    }
}

public sealed class ClaimServiceDeliveryConfiguration
    : IEntityTypeConfiguration<ClaimServiceDelivery>
{
    public void Configure(EntityTypeBuilder<ClaimServiceDelivery> builder)
    {
        builder.ToTable("ClaimServiceDeliveries");
        builder.HasKey(x => new { x.ClaimId, x.ServiceDeliveryId });

        builder.HasOne(x => x.Claim)
            .WithMany(x => x.ServiceDeliveries)
            .HasForeignKey(x => x.ClaimId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.ServiceDelivery)
            .WithMany(x => x.ClaimLinks)
            .HasForeignKey(x => x.ServiceDeliveryId)
            .OnDelete(DeleteBehavior.Restrict);

        // A recorded service delivery can only ever be claimed once.
        builder.HasIndex(x => x.ServiceDeliveryId).IsUnique();
    }
}
