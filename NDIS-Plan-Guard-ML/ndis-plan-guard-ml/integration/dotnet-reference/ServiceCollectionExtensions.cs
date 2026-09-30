using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace NDIS.PlanGuard.RiskScoring.Reference;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPythonRiskScoringReference(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<RiskScoringOptions>(
            configuration.GetSection(RiskScoringOptions.SectionName));

        services.AddHttpClient<PythonRiskScoringClient>((provider, client) =>
        {
            var options = provider.GetRequiredService<IOptions<RiskScoringOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
            client.Timeout = options.Timeout;
        });

        return services;
    }
}
