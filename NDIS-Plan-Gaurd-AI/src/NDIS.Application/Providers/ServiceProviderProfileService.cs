using Microsoft.EntityFrameworkCore;
using NDIS.Application.Abstractions.Identity;
using NDIS.Application.Abstractions.Persistence;
using NDIS.Application.Abstractions.Security;
using NDIS.Application.Common.Validation;
using static NDIS.Domain.Enums.Enums;

namespace NDIS.Application.Providers;

public sealed class ServiceProviderProfileService : IServiceProviderProfileService
{
    private readonly ICurrentUserService _currentUser;
    private readonly IIdentityService _identityService;
    private readonly IAppDbContext _dbContext;

    public ServiceProviderProfileService(
        ICurrentUserService currentUser,
        IIdentityService identityService,
        IAppDbContext dbContext)
    {
        _currentUser = currentUser;
        _identityService = identityService;
        _dbContext = dbContext;
    }

    public async Task<ServiceProviderProfileDto?> GetMyProfileAsync(CancellationToken cancellationToken = default)
    {
        if (_currentUser.UserId is not Guid identityUserId) return null;

        var row = await _dbContext.ServiceProviderUsers
            .AsNoTracking()
            .Where(x => x.UserProfile.IdentityUserId == identityUserId)
            .Select(x => new
            {
                x.ServiceProviderId,
                x.UserProfileId,
                x.UserProfile.FirstName,
                x.UserProfile.LastName,
                x.ServiceProvider.LegalName,
                x.ServiceProvider.TradingName,
                x.ServiceProvider.Abn,
                x.ServiceProvider.NdisRegistrationNumber,
                x.ServiceProvider.ApprovalStatus,
                x.ServiceProvider.Status
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (row is null) return null;
        var identity = await _identityService.FindByIdAsync(identityUserId, cancellationToken);

        return new ServiceProviderProfileDto(
            row.ServiceProviderId,
            row.UserProfileId,
            identity?.Email ?? _currentUser.Email ?? string.Empty,
            row.FirstName,
            row.LastName,
            row.LegalName,
            row.TradingName,
            row.Abn,
            row.NdisRegistrationNumber,
            row.ApprovalStatus.ToString(),
            row.Status.ToString());
    }

    public async Task<ProviderProfileOperationResult> UpdateMyProfileAsync(
        UpdateServiceProviderProfileCommand command,
        CancellationToken cancellationToken = default)
    {
        if (_currentUser.UserId is not Guid identityUserId)
            return ProviderProfileOperationResult.Failure("Authenticated user was not found.");

        var membership = await _dbContext.ServiceProviderUsers
            .Include(x => x.UserProfile)
            .Include(x => x.ServiceProvider)
            .FirstOrDefaultAsync(x => x.UserProfile.IdentityUserId == identityUserId, cancellationToken);

        if (membership is null)
            return ProviderProfileOperationResult.Failure("Service Provider profile was not found.");

        var abn = AbnValidator.Normalize(command.Abn);
        if (!AbnValidator.IsValid(abn))
            return ProviderProfileOperationResult.Failure("A valid Australian ABN is required.");

        var duplicateAbn = await _dbContext.ServiceProviders.AnyAsync(
            x => x.Id != membership.ServiceProviderId && x.Abn == abn,
            cancellationToken);
        if (duplicateAbn)
            return ProviderProfileOperationResult.Failure("Another Service Provider already uses this ABN.");
        var provider = membership.ServiceProvider;
        var tradingName =
    string.IsNullOrWhiteSpace(command.TradingName)
        ? null
        : command.TradingName.Trim();

        var newAbn =
            string.IsNullOrWhiteSpace(command.Abn)
                ? null
                : command.Abn.Trim();

        var ndisRegistrationNumber =
            string.IsNullOrWhiteSpace(command.NdisRegistrationNumber)
                ? null
                : command.NdisRegistrationNumber.Trim();
        if (provider.ApprovalStatus == ApprovalStatus.Approved)
        {
            if (!string.Equals(
                    provider.TradingName,
                    tradingName,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "Trading name cannot be changed after approval.");
            }

            if (!string.Equals(
                    provider.Abn,
                    newAbn,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "ABN cannot be changed after approval.");
            }

            if (!string.Equals(
                    provider.NdisRegistrationNumber,
                    ndisRegistrationNumber,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "NDIS registration number cannot be changed after approval.");
            }
        }

        membership.UserProfile.UpdateName(command.FirstName.Trim(), command.LastName.Trim());
        membership.ServiceProvider.UpdateDetails(
     command.LegalName.Trim(),
     string.IsNullOrWhiteSpace(command.TradingName)
         ? null
         : command.TradingName.Trim(),
     string.IsNullOrWhiteSpace(abn)?null:command.Abn.Trim(),
     string.IsNullOrWhiteSpace(command.NdisRegistrationNumber)
         ? null
         : command.NdisRegistrationNumber.Trim());

        await _dbContext.SaveChangesAsync(cancellationToken);
        return ProviderProfileOperationResult.Success();
    }
}
