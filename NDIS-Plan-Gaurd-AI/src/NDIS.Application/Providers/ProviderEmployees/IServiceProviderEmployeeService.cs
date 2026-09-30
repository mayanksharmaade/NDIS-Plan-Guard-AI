using NDIS.Application.ProviderEmployees.Models;

namespace NDIS.Application.ProviderEmployees;

public interface IServiceProviderEmployeeService
{
    Task<IReadOnlyCollection<ServiceProviderEmployeeDto>> GetByProviderAsync(
        Guid serviceProviderId,
        CancellationToken cancellationToken = default);

    Task<ServiceProviderEmployeeDto> CreateAsync(
        Guid serviceProviderId,
        SaveServiceProviderEmployeeRequest request,
        CancellationToken cancellationToken = default);

    Task<ServiceProviderEmployeeDto?> UpdateAsync(
        Guid serviceProviderId,
        Guid employeeId,
        SaveServiceProviderEmployeeRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> SetActiveAsync(
        Guid serviceProviderId,
        Guid employeeId,
        bool isActive,
        CancellationToken cancellationToken = default);
}
