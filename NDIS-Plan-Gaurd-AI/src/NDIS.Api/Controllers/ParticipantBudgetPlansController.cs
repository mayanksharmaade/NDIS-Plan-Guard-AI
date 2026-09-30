using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NDIS.Application.Common.Security;
using NDIS.Application.BudgetPlans;
using NDIS.Application.BudgetPlans.Models;

namespace NDIS.Api.Controllers;

[ApiController]
[Authorize(Roles = AppRoles.ServiceProvider)]
[Route("api/participants/{participantId:guid}/budget-plans")]
public sealed class ParticipantBudgetPlansController : ControllerBase
{
    private readonly IParticipantBudgetPlanService _service;

    public ParticipantBudgetPlansController(IParticipantBudgetPlanService service)
        => _service = service;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<ParticipantBudgetPlanDto>>> Get(
        Guid participantId,
        CancellationToken cancellationToken)
        => Ok(await _service.GetByParticipantAsync(participantId, cancellationToken));

    [HttpPost]
    public async Task<ActionResult<ParticipantBudgetPlanDto>> Create(
        Guid participantId,
        SaveParticipantBudgetPlanRequest request,
        CancellationToken cancellationToken)
        => Ok(await _service.CreateAsync(
            participantId,
            request,
            cancellationToken));

    [HttpPost("{budgetPlanId:guid}/allocations")]
    public async Task<ActionResult<ParticipantBudgetAllocationDto>> AddAllocation(
        Guid participantId,
        Guid budgetPlanId,
        AddBudgetAllocationRequest request,
        CancellationToken cancellationToken)
        => Ok(await _service.AddAllocationAsync(
            participantId,
            budgetPlanId,
            request,
            cancellationToken));
}
