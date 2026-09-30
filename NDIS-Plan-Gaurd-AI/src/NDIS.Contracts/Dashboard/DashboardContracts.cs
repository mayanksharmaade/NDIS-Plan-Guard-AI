namespace NDIS.Contracts.Dashboard;

public sealed record DashboardRecentClaimResponse(
    Guid ClaimId,
    string ClaimNumber,
    string ParticipantName,
    string? ServiceProviderName,
    decimal Amount,
    string Status,
    DateTime CreatedAtUtc,
    DateTime? SubmittedAtUtc);

public sealed record DashboardSummaryResponse(
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
    IReadOnlyCollection<DashboardRecentClaimResponse> RecentClaims);
