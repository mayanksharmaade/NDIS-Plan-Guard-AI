using Microsoft.EntityFrameworkCore;
using NDIS.Application.Abstractions.Identity;
using NDIS.Application.Abstractions.Persistence;
using NDIS.Application.Common.Security;
using NDIS.Domain.Entities;
using static NDIS.Domain.Enums.Enums;

namespace NDIS.Application.Administration;

public sealed class UserAdministrationService : IUserAdministrationService
{
    private readonly IIdentityService _identityService;
    private readonly IAppDbContext _dbContext;
    private readonly ITransactionManager _transactionManager;

    public UserAdministrationService(
        IIdentityService identityService,
        IAppDbContext dbContext,
        ITransactionManager transactionManager)
    {
        _identityService = identityService;
        _dbContext = dbContext;
        _transactionManager = transactionManager;
    }

    public async Task<CreateAdminResult> CreateAdminAsync(
        CreateAdminCommand command,
        CancellationToken cancellationToken = default)
    {
        var email = command.Email.Trim().ToLowerInvariant();
        if (await _identityService.EmailExistsAsync(email, cancellationToken))
            return CreateAdminResult.Failure("A user with this email already exists.");

        return await _transactionManager.ExecuteAsync(async token =>
        {
            var created = await _identityService.CreateUserAsync(email, command.Password, command.PhoneNumber, token);
            if (!created.Succeeded || created.UserId is null)
                return CreateAdminResult.Failure(created.Errors.ToArray());

            var roleResult = await _identityService.AddToRoleAsync(created.UserId.Value, AppRoles.Admin, token);
            if (!roleResult.Succeeded)
                throw new InvalidOperationException(string.Join("; ", roleResult.Errors));

            var profile = new UserProfile(
                created.UserId.Value,
                command.FirstName.Trim(),
                command.LastName.Trim(),
                UserCategory.Internal);
            profile.Approve();

            _dbContext.UserProfiles.Add(profile);
            await _dbContext.SaveChangesAsync(token);
            return CreateAdminResult.Success(profile.Id);
        }, cancellationToken);
    }

    public async Task<IReadOnlyCollection<PendingRegistrationDto>> GetPendingRegistrationsAsync(
        CancellationToken cancellationToken = default)
    {
        var rows = await _dbContext.ServiceProviderUsers
            .AsNoTracking()
            .Where(x => x.UserProfile.ApprovalStatus == ApprovalStatus.Pending)
            .Select(x => new
            {
                x.UserProfileId,
                x.UserProfile.IdentityUserId,
                x.UserProfile.FirstName,
                x.UserProfile.LastName,
                x.ServiceProviderId,
                x.ServiceProvider.LegalName,
                x.ServiceProvider.TradingName,
                x.ServiceProvider.Abn,
                x.ServiceProvider.NdisRegistrationNumber,
                x.UserProfile.CreatedAtUtc
            })
            .OrderBy(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        var result = new List<PendingRegistrationDto>(rows.Count);
        foreach (var row in rows)
        {
            var identity = await _identityService.FindByIdAsync(row.IdentityUserId, cancellationToken);
            result.Add(new PendingRegistrationDto(
                row.UserProfileId,
                row.ServiceProviderId,
                identity?.Email ?? string.Empty,
                row.FirstName,
                row.LastName,
                row.LegalName,
                row.TradingName,
                row.Abn,
                row.NdisRegistrationNumber,
                row.CreatedAtUtc));
        }

        return result;
    }

    public async Task<IReadOnlyCollection<UserSummaryDto>> GetUsersAsync(CancellationToken cancellationToken = default)
    {
        var profiles = await _dbContext.UserProfiles
            .AsNoTracking()
            .OrderBy(x => x.FirstName)
            .ThenBy(x => x.LastName)
            .ToListAsync(cancellationToken);

        var result = new List<UserSummaryDto>(profiles.Count);
        foreach (var profile in profiles)
        {
            var identity = await _identityService.FindByIdAsync(profile.IdentityUserId, cancellationToken);
            var roles = await _identityService.GetRolesAsync(profile.IdentityUserId, cancellationToken);
            result.Add(new UserSummaryDto(
                profile.Id,
                profile.IdentityUserId,
                identity?.Email ?? string.Empty,
                profile.FirstName,
                profile.LastName,
                profile.UserCategory.ToString(),
                profile.ApprovalStatus.ToString(),
                profile.AccountStatus.ToString(),
                roles));
        }

        return result;
    }

    public Task<AdminOperationResult> ApproveRegistrationAsync(Guid userProfileId, CancellationToken cancellationToken = default) =>
        UpdateRegistrationAsync(userProfileId, approve: true, cancellationToken);

    public Task<AdminOperationResult> RejectRegistrationAsync(Guid userProfileId, CancellationToken cancellationToken = default) =>
        UpdateRegistrationAsync(userProfileId, approve: false, cancellationToken);

    public async Task<AdminOperationResult> ActivateUserAsync(Guid userProfileId, CancellationToken cancellationToken = default)
    {
        var profile = await _dbContext.UserProfiles.SingleOrDefaultAsync(x => x.Id == userProfileId, cancellationToken);
        if (profile is null) return AdminOperationResult.Failure("User was not found.");

        try { profile.Activate(); }
        catch (InvalidOperationException ex) { return AdminOperationResult.Failure(ex.Message); }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return AdminOperationResult.Success();
    }

    public async Task<AdminOperationResult> DisableUserAsync(Guid userProfileId, CancellationToken cancellationToken = default)
    {
        var profile = await _dbContext.UserProfiles.SingleOrDefaultAsync(x => x.Id == userProfileId, cancellationToken);
        if (profile is null) return AdminOperationResult.Failure("User was not found.");

        profile.Disable();
        await _dbContext.SaveChangesAsync(cancellationToken);
        return AdminOperationResult.Success();
    }

    private async Task<AdminOperationResult> UpdateRegistrationAsync(
        Guid userProfileId,
        bool approve,
        CancellationToken cancellationToken)
    {
        var membership = await _dbContext.ServiceProviderUsers
            .Include(x => x.UserProfile)
            .Include(x => x.ServiceProvider)
            .SingleOrDefaultAsync(x => x.UserProfileId == userProfileId, cancellationToken);

        if (membership is null)
            return AdminOperationResult.Failure("Service Provider registration was not found.");

        if (membership.UserProfile.ApprovalStatus != ApprovalStatus.Pending)
            return AdminOperationResult.Failure("Registration has already been processed.");

        if (approve)
        {
            membership.UserProfile.Approve();
            membership.ServiceProvider.Approve();
            membership.Activate();
        }
        else
        {
            membership.UserProfile.Reject();
            membership.ServiceProvider.Reject();
            membership.Remove();
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return AdminOperationResult.Success();
    }
}
