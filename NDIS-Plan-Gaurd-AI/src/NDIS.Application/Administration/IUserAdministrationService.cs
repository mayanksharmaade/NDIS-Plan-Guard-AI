namespace NDIS.Application.Administration;

public interface IUserAdministrationService
{
    Task<CreateAdminResult> CreateAdminAsync(CreateAdminCommand command, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<PendingRegistrationDto>> GetPendingRegistrationsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<UserSummaryDto>> GetUsersAsync(CancellationToken cancellationToken = default);
    Task<AdminOperationResult> ApproveRegistrationAsync(Guid userProfileId, CancellationToken cancellationToken = default);
    Task<AdminOperationResult> RejectRegistrationAsync(Guid userProfileId, CancellationToken cancellationToken = default);
    Task<AdminOperationResult> ActivateUserAsync(Guid userProfileId, CancellationToken cancellationToken = default);
    Task<AdminOperationResult> DisableUserAsync(Guid userProfileId, CancellationToken cancellationToken = default);
}
