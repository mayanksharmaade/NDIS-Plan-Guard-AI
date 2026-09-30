namespace NDIS.Application.Abstractions.Identity;

public interface IIdentityService
{
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);

    Task<IdentityUserCreationResult> CreateUserAsync(
        string email,
        string password,
        string? phoneNumber,
        CancellationToken cancellationToken = default);

    Task<IdentityOperationResult> AddToRoleAsync(
        Guid userId,
        string role,
        CancellationToken cancellationToken = default);

    Task<IdentityUserInfo?> FindByEmailAsync(
        string email,
        CancellationToken cancellationToken = default);

    Task<IdentityUserInfo?> FindByIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<bool> CheckPasswordAsync(
        Guid userId,
        string password,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<string>> GetRolesAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
