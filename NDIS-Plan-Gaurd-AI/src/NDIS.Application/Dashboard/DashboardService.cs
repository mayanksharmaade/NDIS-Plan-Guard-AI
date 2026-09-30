using Microsoft.EntityFrameworkCore;
using NDIS.Application.Abstractions.Persistence;
using NDIS.Application.Abstractions.Security;
using NDIS.Application.Common.Security;
using static NDIS.Domain.Enums.Enums;

namespace NDIS.Application.Dashboard;

public sealed class DashboardService : IDashboardService
{
    private readonly IAppDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;

    public DashboardService(IAppDbContext dbContext, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync(
        CancellationToken cancellationToken = default)
    {
        Guid? providerId = null;
        var scope = "Internal";

        if (_currentUser.IsInRole(AppRoles.ServiceProvider))
        {
            scope = "ServiceProvider";

            if (_currentUser.UserId is Guid identityUserId)
            {
                providerId = await CurrentProviderResolver.ResolveAsync(
                    _dbContext,
                    identityUserId,
                    cancellationToken);
            }
        }

        var participants = _dbContext.Participants.AsNoTracking().AsQueryable();
        var claims = _dbContext.Claims.AsNoTracking().AsQueryable();

        if (scope == "ServiceProvider")
        {
            if (providerId is null)
            {
                participants = participants.Where(_ => false);
                claims = claims.Where(_ => false);
            }
            else
            {
                participants = participants.Where(x => x.ServiceProviderId == providerId.Value);
                claims = claims.Where(x => x.ServiceProviderId == providerId.Value);
            }
        }

        var totalParticipants = await participants.CountAsync(cancellationToken);
        var activeParticipants = await participants.CountAsync(
            x => x.Status == ParticipantStatus.Active,
            cancellationToken);

        var totalClaims = await claims.CountAsync(cancellationToken);
        var draftClaims = await claims.CountAsync(x => x.Status == ClaimStatus.Draft, cancellationToken);
        var submittedClaims = await claims.CountAsync(
            x => x.Status == ClaimStatus.Submitted || x.Status == ClaimStatus.ReviewRequired,
            cancellationToken);
        var inReviewClaims = await claims.CountAsync(x => x.Status == ClaimStatus.Processing, cancellationToken);
        var moreInfoClaims = await claims.CountAsync(
            x => x.Status == ClaimStatus.MoreInformationRequired,
            cancellationToken);
        var approvedClaims = await claims.CountAsync(x => x.Status == ClaimStatus.Approved, cancellationToken);
        var rejectedClaims = await claims.CountAsync(x => x.Status == ClaimStatus.Rejected, cancellationToken);

        var totalClaimAmount = await claims
            .Where(x => x.Status != ClaimStatus.Cancelled)
            .SumAsync(x => (decimal?)x.Amount, cancellationToken) ?? 0m;

        var approvedAmount = await claims
            .Where(x => x.Status == ClaimStatus.Approved)
            .SumAsync(x => (decimal?)x.Amount, cancellationToken) ?? 0m;

        var pendingProviderRegistrations = scope == "ServiceProvider"
            ? 0
            : await _dbContext.ServiceProviders.AsNoTracking().CountAsync(
                x => x.ApprovalStatus == ApprovalStatus.Pending,
                cancellationToken);

        var activeServiceProviders = scope == "ServiceProvider"
            ? (providerId.HasValue ? 1 : 0)
            : await _dbContext.ServiceProviders.AsNoTracking().CountAsync(
                x => x.Status == ServiceProviderStatus.Active,
                cancellationToken);

        var recentQuery = _dbContext.Claims
            .AsNoTracking()
            .Include(x => x.Participant)
            .Include(x => x.ServiceProvider)
            .AsQueryable();

        if (scope == "ServiceProvider")
        {
            recentQuery = providerId.HasValue
                ? recentQuery.Where(x => x.ServiceProviderId == providerId.Value)
                : recentQuery.Where(_ => false);
        }

        var recentRows = await recentQuery
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(8)
            .ToListAsync(cancellationToken);

        var recentClaims = recentRows.Select(x => new DashboardRecentClaimDto(
            x.Id,
            x.ClaimNumber,
            $"{x.Participant.FirstName} {x.Participant.LastName}".Trim(),
            scope == "ServiceProvider"
                ? null
                : (string.IsNullOrWhiteSpace(x.ServiceProvider.TradingName)
                    ? x.ServiceProvider.LegalName
                    : x.ServiceProvider.TradingName),
            x.Amount,
            x.Status.ToString(),
            x.CreatedAtUtc,
            x.SubmittedAtUtc)).ToArray();

        return new DashboardSummaryDto(
            scope,
            totalParticipants,
            activeParticipants,
            totalClaims,
            draftClaims,
            submittedClaims,
            inReviewClaims,
            moreInfoClaims,
            approvedClaims,
            rejectedClaims,
            totalClaimAmount,
            approvedAmount,
            pendingProviderRegistrations,
            activeServiceProviders,
            recentClaims);
    }
}
