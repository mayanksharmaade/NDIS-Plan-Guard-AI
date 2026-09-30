using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NDIS.Application.Common.Security;
using NDIS.Application.Dashboard;
using NDIS.Contracts.Dashboard;

namespace NDIS.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize(Roles = AppRoles.ClaimReaders)]
public sealed class DashboardController : ControllerBase
{
    private readonly IDashboardService _service;

    public DashboardController(IDashboardService service) => _service = service;

    [HttpGet("summary")]
    public async Task<IActionResult> Summary(CancellationToken cancellationToken)
    {
        var x = await _service.GetSummaryAsync(cancellationToken);
        return Ok(new DashboardSummaryResponse(
            x.Scope,
            x.TotalParticipants,
            x.ActiveParticipants,
            x.TotalClaims,
            x.DraftClaims,
            x.SubmittedClaims,
            x.InReviewClaims,
            x.MoreInformationRequiredClaims,
            x.ApprovedClaims,
            x.RejectedClaims,
            x.TotalClaimAmount,
            x.ApprovedAmount,
            x.PendingProviderRegistrations,
            x.ActiveServiceProviders,
            x.RecentClaims.Select(c => new DashboardRecentClaimResponse(
                c.ClaimId,
                c.ClaimNumber,
                c.ParticipantName,
                c.ServiceProviderName,
                c.Amount,
                c.Status,
                c.CreatedAtUtc,
                c.SubmittedAtUtc)).ToArray()));
    }
}
