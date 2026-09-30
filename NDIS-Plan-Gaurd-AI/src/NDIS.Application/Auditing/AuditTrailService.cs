using Microsoft.EntityFrameworkCore;
using NDIS.Application.Abstractions.Persistence;
using NDIS.Application.Abstractions.Security;
using NDIS.Application.Common.Security;
using NDIS.Domain.Entities;

namespace NDIS.Application.Auditing;

public sealed class AuditTrailService : IAuditTrailService
{
    private readonly IAppDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;

    public AuditTrailService(IAppDbContext dbContext, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public void Add(AddAuditEventCommand command)
    {
        var auditEvent = new AuditEvent(
            command.ServiceProviderId,
            command.EntityType,
            command.EntityId,
            command.Action,
            command.Description,
            _currentUser.UserId,
            _currentUser.Email);

        _dbContext.AuditEvents.Add(auditEvent);
    }

    public async Task<IReadOnlyCollection<AuditEventDto>> GetEventsAsync(
        int take = 100,
        string? entityType = null,
        Guid? entityId = null,
        CancellationToken cancellationToken = default)
    {
        take = Math.Clamp(take, 1, 200);
        var query = _dbContext.AuditEvents.AsNoTracking().AsQueryable();

        if (_currentUser.IsInRole(AppRoles.ServiceProvider))
        {
            if (_currentUser.UserId is not Guid identityUserId)
                return Array.Empty<AuditEventDto>();

            var providerId = await CurrentProviderResolver.ResolveAsync(
                _dbContext,
                identityUserId,
                cancellationToken);

            if (providerId is null)
                return Array.Empty<AuditEventDto>();

            query = query.Where(x => x.ServiceProviderId == providerId.Value);
        }

        if (!string.IsNullOrWhiteSpace(entityType))
        {
            var trimmedEntityType = entityType.Trim();
            query = query.Where(x => x.EntityType == trimmedEntityType);
        }

        if (entityId.HasValue)
            query = query.Where(x => x.EntityId == entityId.Value);

        var rows = await query
            .OrderByDescending(x => x.OccurredAtUtc)
            .Take(take)
            .ToListAsync(cancellationToken);

        return rows.Select(x => new AuditEventDto(
            x.Id,
            x.ServiceProviderId,
            x.EntityType,
            x.EntityId,
            x.Action.ToString(),
            x.Description,
            x.ActorIdentityUserId,
            x.ActorEmail,
            x.OccurredAtUtc)).ToArray();
    }
}
