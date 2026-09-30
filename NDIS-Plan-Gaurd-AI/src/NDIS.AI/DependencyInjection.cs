using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace NDIS.AI;

public static class DependencyInjection
{
    public static IServiceCollection AddAI(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        return services;
    }
}