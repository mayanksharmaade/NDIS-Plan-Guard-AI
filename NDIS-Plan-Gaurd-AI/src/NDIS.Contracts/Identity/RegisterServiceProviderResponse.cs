namespace NDIS.Contracts.Identity;

public sealed record RegisterServiceProviderResponse(
    Guid UserProfileId,
    Guid ServiceProviderId,
    string Email,
    string ApprovalStatus,
    string Message);
