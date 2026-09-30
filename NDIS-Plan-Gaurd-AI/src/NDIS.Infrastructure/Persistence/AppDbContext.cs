using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NDIS.Application.Abstractions.Persistence;
using NDIS.Domain.Entities;
using NDIS.Infrastructure.Identity;

namespace NDIS.Infrastructure.Persistence;

public class AppDbContext
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<ServiceProvider> ServiceProviders => Set<ServiceProvider>();
    public DbSet<ServiceProviderUser> ServiceProviderUsers => Set<ServiceProviderUser>();
    public DbSet<Participant> Participants => Set<Participant>();
    public DbSet<Claim> Claims => Set<Claim>();
    public DbSet<ClaimReview> ClaimReviews => Set<ClaimReview>();
    public DbSet<AuditEvent> AuditEvents => Set<AuditEvent>();
    public DbSet<ClaimRiskAssessment> ClaimRiskAssessments => Set<ClaimRiskAssessment>();
    public DbSet<ClaimRiskFactor> ClaimRiskFactors => Set<ClaimRiskFactor>();
    public DbSet<ServiceProviderEmployee> ServiceProviderEmployees
    => Set<ServiceProviderEmployee>();

    public DbSet<ParticipantServiceAssignment> ParticipantServiceAssignments
        => Set<ParticipantServiceAssignment>();

    public DbSet<ServiceDelivery> ServiceDeliveries
        => Set<ServiceDelivery>();

    public DbSet<ParticipantBudgetPlan> ParticipantBudgetPlans
        => Set<ParticipantBudgetPlan>();

    public DbSet<ParticipantBudgetAllocation> ParticipantBudgetAllocations
        => Set<ParticipantBudgetAllocation>();

    public DbSet<BudgetPlanTemplate> BudgetPlanTemplates
        => Set<BudgetPlanTemplate>();

    public DbSet<ClaimServiceDelivery> ClaimServiceDeliveries
        => Set<ClaimServiceDelivery>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
