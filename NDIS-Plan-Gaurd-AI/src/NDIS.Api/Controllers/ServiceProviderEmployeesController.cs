using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NDIS.Application.Common.Security;
using NDIS.Application.ProviderEmployees;
using NDIS.Application.ProviderEmployees.Models;

namespace NDIS.Api.Controllers;

[ApiController]
[Authorize(Roles = AppRoles.ServiceProvider)]
[Route("api/service-providers/{serviceProviderId:guid}/employees")]
public sealed class ServiceProviderEmployeesController : ControllerBase
{
    private readonly IServiceProviderEmployeeService _service;

    public ServiceProviderEmployeesController(IServiceProviderEmployeeService service)
        => _service = service;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<ServiceProviderEmployeeDto>>> Get(
        Guid serviceProviderId,
        CancellationToken cancellationToken)
        => Ok(await _service.GetByProviderAsync(serviceProviderId, cancellationToken));

    [HttpPost]
    public async Task<ActionResult<ServiceProviderEmployeeDto>> Create(
        Guid serviceProviderId,
        SaveServiceProviderEmployeeRequest request,
        CancellationToken cancellationToken)
        => Ok(await _service.CreateAsync(
            serviceProviderId,
            request,
            cancellationToken));

    [HttpPut("{employeeId:guid}")]
    public async Task<ActionResult<ServiceProviderEmployeeDto>> Update(
        Guid serviceProviderId,
        Guid employeeId,
        SaveServiceProviderEmployeeRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _service.UpdateAsync(
            serviceProviderId,
            employeeId,
            request,
            cancellationToken);

        return result is null ? NotFound() : Ok(result);
    }

    [HttpPatch("{employeeId:guid}/active")]
    public async Task<IActionResult> SetActive(
        Guid serviceProviderId,
        Guid employeeId,
        [FromBody] bool isActive,
        CancellationToken cancellationToken)
    {
        var updated = await _service.SetActiveAsync(
            serviceProviderId,
            employeeId,
            isActive,
            cancellationToken);

        return updated ? NoContent() : NotFound();
    }
}
