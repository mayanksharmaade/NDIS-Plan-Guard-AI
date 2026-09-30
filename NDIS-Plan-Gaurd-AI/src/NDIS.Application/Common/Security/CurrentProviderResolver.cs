using Microsoft.EntityFrameworkCore;
using NDIS.Application.Abstractions.Persistence;
using static NDIS.Domain.Enums.Enums;

namespace NDIS.Application.Common.Security;

internal static class CurrentProviderResolver
{
    public static Task<Guid?> ResolveAsync(
        IAppDbContext dbContext,
        Guid identityUserId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.ServiceProviderUsers
            .AsNoTracking()
            .Where(x =>
                x.UserProfile.IdentityUserId == identityUserId &&
                x.Status == ProviderMembershipStatus.Active &&
                x.ServiceProvider.ApprovalStatus == ApprovalStatus.Approved &&
                x.ServiceProvider.Status == ServiceProviderStatus.Active)
            .Select(x => (Guid?)x.ServiceProviderId)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
