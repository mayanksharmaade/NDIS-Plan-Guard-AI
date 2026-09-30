using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NDIS.Application.Claims;
using NDIS.Application.Abstractions.Risk;
using NDIS.Application.Common.Security;
using NDIS.Contracts.Claims;
using NDIS.Contracts.Risk;

namespace NDIS.Api.Controllers;

[ApiController]
[Route("api/claims")]
[Authorize(Roles = AppRoles.ClaimReaders)]
public sealed class ClaimsController : ControllerBase
{
    private readonly IClaimQueryService _queryService;
    private readonly IClaimCommandService _commandService;
    private readonly IClaimRiskAssessmentService _riskAssessmentService;

    public ClaimsController(
        IClaimQueryService queryService,
        IClaimCommandService commandService,
        IClaimRiskAssessmentService riskAssessmentService)
    {
        _queryService = queryService;
        _commandService = commandService;
        _riskAssessmentService = riskAssessmentService;
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var rows = await _queryService.GetClaimsAsync(cancellationToken);
        return Ok(rows.Select(x => new ClaimListItemResponse(
            x.Id,
            x.ClaimNumber,
            x.ParticipantId,
            x.ParticipantName,
            x.ServiceFrom,
            x.ServiceTo,
            x.SupportCategory,
            x.Amount,
            x.Units,
            x.UnitPrice,
            x.Status,
            x.CreatedAtUtc,
            x.SubmittedAtUtc)));
    }

    [HttpGet("eligible-deliveries")]
    [Authorize(Roles = AppRoles.ServiceProvider)]
    public async Task<IActionResult> EligibleDeliveries(
        [FromQuery] Guid participantId,
        [FromQuery] DateTime serviceFrom,
        [FromQuery] DateTime serviceTo,
        CancellationToken cancellationToken)
    {
        var rows = await _queryService.GetEligibleServiceDeliveriesAsync(
            participantId,
            serviceFrom,
            serviceTo,
            cancellationToken);

        return Ok(rows.Select(x => new EligibleServiceDeliveryResponse(
            x.Id,
            x.ParticipantServiceAssignmentId,
            x.ServiceProviderEmployeeId,
            x.EmployeeName,
            x.SupportCategory,
            x.ServiceStartUtc,
            x.ServiceEndUtc,
            x.ServiceHours,
            x.AgreedHourlyRate,
            x.Amount,
            x.ServiceLocation)));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var x = await _queryService.GetByIdAsync(id, cancellationToken);
        return x is null
            ? NotFound()
            : Ok(new ClaimDetailsResponse(
                x.Id,
                x.ClaimNumber,
                x.ServiceProviderId,
                x.ParticipantId,
                x.ParticipantNdisNumber,
                x.ParticipantName,
                x.ServiceFrom,
                x.ServiceTo,
                x.SupportCategory,
                x.Description,
                x.Amount,
                x.Units,
                x.UnitPrice,
                x.Status,
                x.CreatedAtUtc,
                x.SubmittedAtUtc,
                MapRisk(x.RiskAssessment),
                x.TotalServiceHours,
                x.AgreedHourlyRate,
                x.EmployeeName,
                (x.ServiceDeliveries ?? Array.Empty<ClaimServiceDeliveryDto>())
                    .Select(d => new ClaimServiceDeliveryResponse(
                        d.Id,
                        d.ServiceProviderEmployeeId,
                        d.EmployeeName,
                        d.SupportCategory,
                        d.ServiceStartUtc,
                        d.ServiceEndUtc,
                        d.ServiceHours,
                        d.HourlyRate,
                        d.Amount,
                        d.ServiceLocation))
                    .ToArray()));
    }

    [HttpPost]
    [Authorize(Roles = AppRoles.ServiceProvider)]
    public async Task<IActionResult> Create(
        CreateClaimRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _commandService.CreateAsync(
            new CreateClaimCommand(
                request.ParticipantId,
                request.ServiceFrom,
                request.ServiceTo,
                request.SupportCategory,
                request.Description,
                request.Amount,
                request.Units,
                request.UnitPrice,
                request.ServiceDeliveryIds),
            cancellationToken);

        if (!result.Succeeded)
            return BadRequest(new { error = result.Error });

        return StatusCode(
            StatusCodes.Status201Created,
            new ClaimMutationResponse(
                result.ClaimId!.Value,
                result.ClaimNumber!,
                "Claim draft created."));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = AppRoles.ServiceProvider)]
    public async Task<IActionResult> Update(
        Guid id,
        UpdateClaimRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _commandService.UpdateAsync(
            id,
            new UpdateClaimCommand(
                request.ServiceFrom,
                request.ServiceTo,
                request.SupportCategory,
                request.Description,
                request.Amount,
                request.Units,
                request.UnitPrice),
            cancellationToken);

        return result.Succeeded
            ? NoContent()
            : BadRequest(new { error = result.Error });
    }

    [HttpGet("{id:guid}/risk")]
    public async Task<IActionResult> GetRisk(
        Guid id,
        CancellationToken cancellationToken)
    {
        var claim = await _queryService.GetByIdAsync(id, cancellationToken);
        if (claim is null)
            return NotFound();

        return claim.RiskAssessment is null
            ? NotFound()
            : Ok(MapRisk(claim.RiskAssessment));
    }

    [HttpPost("{id:guid}/risk/rescore")]
    [Authorize(Roles = AppRoles.Reviewers)]
    public async Task<IActionResult> RescoreRisk(
        Guid id,
        CancellationToken cancellationToken)
    {
        var claim = await _queryService.GetByIdAsync(id, cancellationToken);
        if (claim is null)
            return NotFound();

        var risk = await _riskAssessmentService.ScoreAndPersistAsync(
            id,
            HttpContext.TraceIdentifier,
            cancellationToken);

        return Ok(MapRisk(risk));
    }

    [HttpPost("{id:guid}/submit")]
    [Authorize(Roles = AppRoles.ServiceProvider)]
    public async Task<IActionResult> Submit(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _commandService.SubmitAsync(id, cancellationToken);
        return result.Succeeded
            ? Ok(new ClaimMutationResponse(
                result.ClaimId!.Value,
                result.ClaimNumber!,
                "Claim submitted for review."))
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
