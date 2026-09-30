using Microsoft.EntityFrameworkCore;
using NDIS.Application.Abstractions.Persistence;
using NDIS.Application.Abstractions.Security;
using NDIS.Application.Common.Security;
using NDIS.Application.ProviderEmployees.Models;
using NDIS.Domain.Entities;
using NDIS.Domain.Enums;


namespace NDIS.Application.ProviderEmployees;

public sealed class ServiceProviderEmployeeService
    : IServiceProviderEmployeeService
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public ServiceProviderEmployeeService(IAppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyCollection<ServiceProviderEmployeeDto>>
        GetByProviderAsync(
            Guid serviceProviderId,
            CancellationToken cancellationToken = default)
    {
        await EnsureCurrentProviderAsync(serviceProviderId, cancellationToken);

        return await _db.ServiceProviderEmployees
            .AsNoTracking()
            .Where(x =>
                x.ServiceProviderId == serviceProviderId)
            .OrderBy(x => x.FirstName)
            .ThenBy(x => x.LastName)
            .Select(x => ToDto(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<ServiceProviderEmployeeDto> CreateAsync(
        Guid serviceProviderId,
        SaveServiceProviderEmployeeRequest request,
        CancellationToken cancellationToken = default)
    {
        await EnsureCurrentProviderAsync(serviceProviderId, cancellationToken);

        var employeeNumber =
            request.EmployeeNumber.Trim();

        if (!request.DefaultHourlyRate.HasValue || request.DefaultHourlyRate.Value <= 0)
        {
            throw new InvalidOperationException(
                "Employee pay rate is required and must be greater than zero.");
        }


        var duplicate =
            await _db.ServiceProviderEmployees.AnyAsync(
                x =>
                    x.ServiceProviderId == serviceProviderId &&
                    x.EmployeeNumber == employeeNumber,
                cancellationToken);

        if (duplicate)
        {
            throw new InvalidOperationException(
                "Employee number already exists.");
        }

        if (!Enum.TryParse<Enums.EmployeeRole>(
                request.Role,
                true,
                out var role))
        {
            throw new InvalidOperationException(
                "Invalid employee role.");
        }

        var entity =
            new ServiceProviderEmployee
            {
                ServiceProviderId =
                    serviceProviderId,

                EmployeeNumber =
                    employeeNumber,

                FirstName =
                    request.FirstName.Trim(),

                LastName =
                    request.LastName.Trim(),

                Email =
                    request.Email.Trim(),

                Phone =
                    request.Phone?.Trim(),

                Role =
                    role,

                Qualifications =
                    request.Qualifications?.Trim(),

                DefaultHourlyRate =
                    request.DefaultHourlyRate
            };

        _db.ServiceProviderEmployees.Add(entity);

        await _db.SaveChangesAsync(
            cancellationToken);

        return ToDto(entity);
    }

    public async Task<ServiceProviderEmployeeDto?> UpdateAsync(
        Guid serviceProviderId,
        Guid employeeId,
        SaveServiceProviderEmployeeRequest request,
        CancellationToken cancellationToken = default)
    {
        await EnsureCurrentProviderAsync(serviceProviderId, cancellationToken);

        var entity =
            await _db.ServiceProviderEmployees
                .SingleOrDefaultAsync(
                    x =>
                        x.ServiceProviderId ==
                            serviceProviderId &&
                        x.Id == employeeId,
                    cancellationToken);

        if (entity is null)
        {
            return null;
        }

        var employeeNumber =
            request.EmployeeNumber.Trim();

        if (!request.DefaultHourlyRate.HasValue || request.DefaultHourlyRate.Value <= 0)
        {
            throw new InvalidOperationException(
                "Employee pay rate is required and must be greater than zero.");
        }


        var duplicate =
            await _db.ServiceProviderEmployees.AnyAsync(
                x =>
                    x.ServiceProviderId ==
                        serviceProviderId &&
                    x.EmployeeNumber ==
                        employeeNumber &&
                    x.Id != employeeId,
                cancellationToken);

        if (duplicate)
        {
            throw new InvalidOperationException(
                "Employee number already exists.");
        }

        if (!Enum.TryParse<Enums.EmployeeRole>(
                request.Role,
                true,
                out var role))
        {
            throw new InvalidOperationException(
                "Invalid employee role.");
        }

        entity.EmployeeNumber =
            employeeNumber;

        entity.FirstName =
            request.FirstName.Trim();

        entity.LastName =
            request.LastName.Trim();

        entity.Email =
            request.Email.Trim();

        entity.Phone =
            request.Phone?.Trim();

        entity.Role =
            role;

        entity.Qualifications =
            request.Qualifications?.Trim();

        entity.DefaultHourlyRate =
            request.DefaultHourlyRate;

        entity.UpdatedAtUtc =
            DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(
            cancellationToken);

        return ToDto(entity);
    }

    public async Task<bool> SetActiveAsync(
        Guid serviceProviderId,
        Guid employeeId,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        await EnsureCurrentProviderAsync(serviceProviderId, cancellationToken);

        var entity =
            await _db.ServiceProviderEmployees
                .SingleOrDefaultAsync(
                    x =>
                        x.ServiceProviderId ==
                            serviceProviderId &&
                        x.Id == employeeId,
                    cancellationToken);

        if (entity is null)
        {
            return false;
        }

        entity.IsActive =
            isActive;

        entity.UpdatedAtUtc =
            DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(
            cancellationToken);

        return true;
    }

    private static ServiceProviderEmployeeDto ToDto(
        ServiceProviderEmployee x)
    {
        return new ServiceProviderEmployeeDto(
            x.Id,
            x.ServiceProviderId,
            x.EmployeeNumber,
            x.FirstName,
            x.LastName,
            x.Email,
            x.Phone,
            x.Role.ToString(),
            x.Qualifications,
            x.DefaultHourlyRate,
            x.IsActive);
    }

    private async Task EnsureCurrentProviderAsync(Guid serviceProviderId, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not Guid identityUserId)
            throw new InvalidOperationException("Current user was not found.");

        var currentProviderId = await CurrentProviderResolver.ResolveAsync(
            _db,
            identityUserId,
            cancellationToken);

        if (!currentProviderId.HasValue || currentProviderId.Value != serviceProviderId)
            throw new InvalidOperationException("The requested Service Provider does not match the current provider membership.");
    }
}
