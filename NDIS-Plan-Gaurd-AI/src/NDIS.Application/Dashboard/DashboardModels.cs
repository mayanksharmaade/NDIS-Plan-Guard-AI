namespace NDIS.Application.Dashboard;

public sealed record DashboardRecentClaimDto(
    Guid ClaimId,
    string ClaimNumber,
    string ParticipantName,
    string? ServiceProviderName,
    decimal Amount,
    string Status,
    DateTime CreatedAtUtc,
    DateTime? SubmittedAtUtc);

public sealed record DashboardSummaryDto(
    string Scope,
    int TotalParticipants,
    int ActiveParticipants,
    int TotalClaims,
    int DraftClaims,
    int SubmittedClaims,
    int InReviewClaims,
    int MoreInformationRequiredClaims,
    int ApprovedClaims,
    int RejectedClaims,
    decimal TotalClaimAmount,
    decimal ApprovedAmount,
    int PendingProviderRegistrations,
    int ActiveServiceProviders,
    IReadOnlyCollection<DashboardRecentClaimDto> RecentClaims);
