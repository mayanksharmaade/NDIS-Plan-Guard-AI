using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NDIS.Application.BudgetPlanTemplates;
using NDIS.Application.BudgetPlanTemplates.Models;
using NDIS.Application.Common.Security;

namespace NDIS.Api.Controllers;

[ApiController]
[Route("api/budget-plan-templates")]
[Authorize(Roles = AppRoles.ClaimReaders)]
public sealed class BudgetPlanTemplatesController : ControllerBase
{
    private readonly IBudgetPlanTemplateService _service;

    public BudgetPlanTemplatesController(IBudgetPlanTemplateService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var activeOnly = User.IsInRole(AppRoles.ServiceProvider);
        return Ok(await _service.GetAllAsync(activeOnly, cancellationToken));
    }

    [HttpPost]
    [Authorize(Roles = AppRoles.AdminOrSuperAdmin)]
    public async Task<IActionResult> Create(
        SaveBudgetPlanTemplateRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = AppRoles.AdminOrSuperAdmin)]
    public async Task<IActionResult> Update(
        Guid id,
        SaveBudgetPlanTemplateRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _service.UpdateAsync(id, request, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }
}
