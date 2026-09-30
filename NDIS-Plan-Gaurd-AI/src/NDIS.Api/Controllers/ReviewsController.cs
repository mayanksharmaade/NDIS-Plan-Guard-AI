using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NDIS.Application.Abstractions.Risk;
using NDIS.Application.Common.Security;
using NDIS.Application.Reviews;
using NDIS.Application.Reviews.Models;
using NDIS.Contracts.Reviews;
using NDIS.Contracts.Risk;

namespace NDIS.Api.Controllers;

[ApiController]
[Route("api/reviews")]
[Authorize(Roles = AppRoles.Reviewers)]
public sealed class ReviewsController : ControllerBase
{
    private readonly IClaimReviewService _service;
    private readonly IClaimDecisionSupportQueryService
    _decisionSupportQueryService;




    public ReviewsController(
     IClaimReviewService claimReviewService,
     IClaimDecisionSupportQueryService decisionSupportQueryService)
    {
        _service = claimReviewService;
        _decisionSupportQueryService = decisionSupportQueryService;
    }

    [HttpGet("queue")]
    public async Task<IActionResult> Queue(CancellationToken cancellationToken)
    {
        var rows = await _service.GetQueueAsync(cancellationToken);
        return Ok(rows.Select(x => new ReviewQueueItemResponse(
            x.ClaimId,
            x.ClaimNumber,
            x.ServiceProviderId,
            x.ServiceProviderName,
            x.ParticipantId,
            x.ParticipantName,
            x.ParticipantNdisNumber,
            x.ServiceFrom,
            x.ServiceTo,
            x.SupportCategory,
            x.Amount,
            x.Status,
            x.SubmittedAtUtc,
            x.ReviewStartedAtUtc,
            x.RiskAvailable,
            x.RiskScore,
            x.RiskBand)));
    }

    [HttpGet("claims/{id:guid}")]
    public async Task<IActionResult> GetClaim(Guid id, CancellationToken cancellationToken)
    {
        var x = await _service.GetClaimAsync(id, cancellationToken);
        if (x is null)
            return NotFound();

        return Ok(new ClaimForReviewResponse(
            x.ClaimId,
            x.ClaimNumber,
            x.ServiceProviderId,
            x.ServiceProviderName,
            x.ParticipantId,
            x.ParticipantName,
            x.ParticipantNdisNumber,
            x.ServiceFrom,
            x.ServiceTo,
            x.SupportCategory,
            x.Description,
            x.Amount,
            x.Status,
            x.CreatedAtUtc,
            x.SubmittedAtUtc,
            x.ReviewStartedAtUtc,
            x.DecidedAtUtc,
            x.Reviews.Select(r => new ClaimReviewHistoryItemResponse(
                r.Id,
                r.ReviewerIdentityUserId,
                r.ReviewerEmail,
                r.Outcome,
                r.Comments,
                r.ReviewedAtUtc)).ToArray(),
            MapRisk(x.RiskAssessment)));
    }

    [HttpPost("claims/{id:guid}/start")]
    public async Task<IActionResult> Start(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.StartReviewAsync(id, cancellationToken);
        return result.Succeeded
            ? NoContent()
            : BadRequest(new { error = result.Error });
    }


    [HttpGet("{claimId:guid}/decision-support")]
    public async Task<ActionResult<ClaimDecisionSupportDto>> GetDecisionSupport(
    Guid claimId,
    CancellationToken cancellationToken)
    {
        var result =
            await _decisionSupportQueryService.GetAsync(
                claimId,
                cancellationToken);

        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpPost("claims/{id:guid}/decision")]
    public async Task<IActionResult> Decision(
        Guid id,
        RecordClaimDecisionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _service.RecordDecisionAsync(
            id,
            new ReviewDecisionCommand(request.Decision, request.Comments),
            cancellationToken);

        return result.Succeeded
            ? NoContent()
            : BadRequest(new { error = result.Error });
    }

    private static ClaimRiskAssessmentResponse? MapRisk(ClaimRiskAssessmentDto? x) =>
        x is null
            ? null
            : new ClaimRiskAssessmentResponse(
                x.Id,
                x.ClaimId,
                x.Available,
                x.Probability,
                x.Score,
                x.RiskBand,
                x.ModelVersion,
                x.FeatureVersion,
                x.ScoredAtUtc,
                x.FailureReason,
                x.CreatedAtUtc,
                x.Factors.Select(f => new ClaimRiskFactorResponse(
                    f.Feature,
                    f.Description,
                    f.ObservedValue,
                    f.BaselineValue,
                    f.ProbabilityImpact)).ToArray());
}
