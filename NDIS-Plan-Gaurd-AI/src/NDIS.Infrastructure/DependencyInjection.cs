using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using NDIS.Application.Abstractions.Identity;
using NDIS.Application.Abstractions.Persistence;
using NDIS.Application.Abstractions.Security;
using NDIS.Infrastructure.Identity;
using NDIS.Infrastructure.Persistence;
using NDIS.Infrastructure.Security;
using Microsoft.Extensions.Options;
using NDIS.Application.Abstractions.Risk;
using NDIS.Infrastructure.RiskScoring;

namespace NDIS.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

        services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));
        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddDataProtection();
        services.AddHttpContextAccessor();
        services.Configure<RiskScoringOptions>(
    configuration.GetSection(
        RiskScoringOptions.SectionName));
        services.AddHttpClient<PythonRiskScoringClient>(
    (serviceProvider, client) =>
    {
        var options =
            serviceProvider
                .GetRequiredService<
                    IOptions<RiskScoringOptions>>()
                .Value;

        client.BaseAddress =
            new Uri(options.BaseUrl);

        client.Timeout =
            TimeSpan.FromSeconds(
                options.TimeoutSeconds);
    });
        services.AddScoped<
    IRiskScoringService,
    PythonRiskScoringService>();
        services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.User.RequireUniqueEmail = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        var jwtKey = configuration["Jwt:Key"];
        if (string.IsNullOrWhiteSpace(jwtKey) || jwtKey.Length < 32)
            throw new InvalidOperationException("Jwt:Key must be configured and be at least 32 characters long.");

        var issuer = configuration["Jwt:Issuer"] ?? "NDIS.PlanGuardAI";
        var audience = configuration["Jwt:Audience"] ?? "NDIS.PlanGuardAI.Web";

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1)
                };
            });
        services.AddAuthorization();

        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<ITransactionManager, EfTransactionManager>();
        return services;
    }
}
