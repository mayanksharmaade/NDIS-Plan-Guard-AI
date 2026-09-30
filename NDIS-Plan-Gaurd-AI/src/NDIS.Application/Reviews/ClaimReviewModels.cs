using NDIS.Application.Abstractions.Risk;

namespace NDIS.Application.Reviews;

public sealed record ReviewQueueItemDto(
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

public sealed record ClaimReviewHistoryItemDto(
    Guid Id,
    Guid ReviewerIdentityUserId,
    string ReviewerEmail,
    string Outcome,
    string? Comments,
    DateTime ReviewedAtUtc);

public sealed record ClaimForReviewDto(
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
    IReadOnlyCollection<ClaimReviewHistoryItemDto> Reviews,
    ClaimRiskAssessmentDto? RiskAssessment);

public sealed record ReviewDecisionCommand(
    string Decision,
    string? Comments);

public sealed record ClaimReviewOperationResult(
    bool Succeeded,
    string? Error)
{
    public static ClaimReviewOperationResult Success() => new(true, null);
    public static ClaimReviewOperationResult Failure(string error) => new(false, error);
}
