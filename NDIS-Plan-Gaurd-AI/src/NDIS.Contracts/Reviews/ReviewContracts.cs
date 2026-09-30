using NDIS.Contracts.Risk;

namespace NDIS.Contracts.Reviews;

public sealed record ReviewQueueItemResponse(
    Guid ClaimId,
    string ClaimNumber,
    Guid ServiceProviderId,
    string ServiceProviderName,
    Guid ParticipantId,
    string ParticipantName,
    string ParticipantNdisNumber,
    DateTime ServiceFrom,
    DateTime ServiceTo,
    string SupportCategory,
    decimal Amount,
    string Status,
    DateTime? SubmittedAtUtc,
    DateTime? ReviewStartedAtUtc,
    bool RiskAvailable,
    int? RiskScore,
    string? RiskBand);

public sealed record ClaimReviewHistoryItemResponse(
    Guid Id,
    Guid ReviewerIdentityUserId,
    string ReviewerEmail,
    string Outcome,
    string? Comments,
    DateTime ReviewedAtUtc);

public sealed record ClaimForReviewResponse(
    Guid ClaimId,
    string ClaimNumber,
    Guid ServiceProviderId,
    string ServiceProviderName,
    Guid ParticipantId,
    string ParticipantName,
    string ParticipantNdisNumber,
    DateTime ServiceFrom,
    DateTime ServiceTo,
    string SupportCategory,
    string Description,
    decimal Amount,
    string Status,
    DateTime CreatedAtUtc,
    DateTime? SubmittedAtUtc,
    DateTime? ReviewStartedAtUtc,
    DateTime? DecidedAtUtc,
    IReadOnlyCollection<ClaimReviewHistoryItemResponse> Reviews,
    ClaimRiskAssessmentResponse? RiskAssessment);

public sealed record RecordClaimDecisionRequest(
    string Decision,
    string? Comments);
