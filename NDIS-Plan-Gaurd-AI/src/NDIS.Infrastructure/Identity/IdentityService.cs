using Microsoft.AspNetCore.Identity;
using NDIS.Application.Abstractions.Identity;

namespace NDIS.Infrastructure.Identity;

public sealed class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public IdentityService(UserManager<ApplicationUser> userManager) => _userManager = userManager;

    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return await _userManager.FindByEmailAsync(email) is not null;
    }

    public async Task<IdentityUserCreationResult> CreateUserAsync(
        string email, string password, string? phoneNumber, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            PhoneNumber = string.IsNullOrWhiteSpace(phoneNumber) ? null : phoneNumber.Trim(),
            LockoutEnabled = true
        };

        var result = await _userManager.CreateAsync(user, password);
        return result.Succeeded
            ? IdentityUserCreationResult.Success(user.Id)
            : IdentityUserCreationResult.Failure(result.Errors.Select(x => x.Description));
    }

    public async Task<IdentityOperationResult> AddToRoleAsync(
        Guid userId, string role, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null) return IdentityOperationResult.Failure(["Identity user was not found."]);
        var result = await _userManager.AddToRoleAsync(user, role);
        return result.Succeeded
            ? IdentityOperationResult.Success()
            : IdentityOperationResult.Failure(result.Errors.Select(x => x.Description));
    }

    public async Task<IdentityUserInfo?> FindByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var user = await _userManager.FindByEmailAsync(email);
        return user is null ? null : Map(user);
    }

    public async Task<IdentityUserInfo?> FindByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var user = await _userManager.FindByIdAsync(userId.ToString());
        return user is null ? null : Map(user);
    }

    public async Task<bool> CheckPasswordAsync(Guid userId, string password, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var user = await _userManager.FindByIdAsync(userId.ToString());
        return user is not null && await _userManager.CheckPasswordAsync(user, password);
    }

    public async Task<IReadOnlyCollection<string>> GetRolesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null) return Array.Empty<string>();
        var roles = await _userManager.GetRolesAsync(user);
        return roles.ToArray();
    }

    private static IdentityUserInfo Map(ApplicationUser user) =>
        new(user.Id, user.Email ?? user.UserName ?? string.Empty, user.PhoneNumber);
}
