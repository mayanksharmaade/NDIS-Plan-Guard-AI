namespace NDIS.Application.Registration;

public interface IRegistrationService
{
    Task<RegisterServiceProviderResult> RegisterServiceProviderAsync(
        RegisterServiceProviderCommand command,
        CancellationToken cancellationToken = default);
}
