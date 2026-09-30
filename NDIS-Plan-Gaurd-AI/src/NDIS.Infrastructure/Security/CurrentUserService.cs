using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using NDIS.Application.Abstractions.Security;

namespace NDIS.Infrastructure.Security;

public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor) => _httpContextAccessor = httpContextAccessor;

    private ClaimsPrincipal? Principal => _httpContextAccessor.HttpContext?.User;
    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated == true;

    public Guid? UserId => Guid.TryParse(Principal?.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;
    public string? Email => Principal?.FindFirstValue(ClaimTypes.Email);
    public IReadOnlyCollection<string> Roles => Principal?.FindAll(ClaimTypes.Role).Select(x => x.Value).Distinct().ToArray() ?? Array.Empty<string>();
    public bool IsInRole(string role) => Principal?.IsInRole(role) == true;
}
