using Microsoft.EntityFrameworkCore;
using NDIS.Domain.Entities;

namespace NDIS.Application.Abstractions.Persistence;

public interface IAppDbContext
{
    DbSet<UserProfile> UserProfiles { get; }
    DbSet<ServiceProvider> ServiceProviders { get; }
    DbSet<ServiceProviderUser> ServiceProviderUsers { get; }
    DbSet<Participant> Participants { get; }
    DbSet<Claim> Claims { get; }
    DbSet<ClaimReview> ClaimReviews { get; }
    DbSet<AuditEvent> AuditEvents { get; }
    DbSet<ClaimRiskAssessment> ClaimRiskAssessments { get; }
    DbSet<ClaimRiskFactor> ClaimRiskFactors { get; }
    DbSet<ServiceProviderEmployee> ServiceProviderEmployees { get; }

    DbSet<ParticipantServiceAssignment> ParticipantServiceAssignments { get; }

    DbSet<ServiceDelivery> ServiceDeliveries { get; }

    DbSet<ParticipantBudgetPlan> ParticipantBudgetPlans { get; }

    DbSet<ParticipantBudgetAllocation> ParticipantBudgetAllocations { get; }
    DbSet<BudgetPlanTemplate> BudgetPlanTemplates { get; }
    DbSet<ClaimServiceDelivery> ClaimServiceDeliveries { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
