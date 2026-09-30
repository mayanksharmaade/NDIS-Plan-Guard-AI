namespace NDIS.Application.Providers;

public interface IServiceProviderProfileService
{
    Task<ServiceProviderProfileDto?> GetMyProfileAsync(CancellationToken cancellationToken = default);
    Task<ProviderProfileOperationResult> UpdateMyProfileAsync(
        UpdateServiceProviderProfileCommand command,
        CancellationToken cancellationToken = default);
}
