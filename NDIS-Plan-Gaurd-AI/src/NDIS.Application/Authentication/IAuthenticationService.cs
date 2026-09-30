namespace NDIS.Application.Authentication;

public interface IAuthenticationService
{
    Task<LoginResult> LoginAsync(LoginCommand command, CancellationToken cancellationToken = default);
}
