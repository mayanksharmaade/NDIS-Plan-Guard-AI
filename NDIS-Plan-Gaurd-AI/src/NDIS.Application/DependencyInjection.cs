using Microsoft.Extensions.DependencyInjection;
using NDIS.Application.Abstractions.Risk;
using NDIS.Application.Administration;
using NDIS.Application.Auditing;
using NDIS.Application.Authentication;
using NDIS.Application.BudgetPlans;
using NDIS.Application.BudgetPlanTemplates;
using NDIS.Application.Claims;
using NDIS.Application.Dashboard;
using NDIS.Application.Participants;
using NDIS.Application.ParticipantServices;
using NDIS.Application.ProviderEmployees;
using NDIS.Application.Providers;
using NDIS.Application.Registration;
using NDIS.Application.Reviews;
using NDIS.Application.Risk;


namespace NDIS.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IRegistrationService, RegistrationService>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IUserAdministrationService, UserAdministrationService>();
        services.AddScoped<IServiceProviderProfileService, ServiceProviderProfileService>();
        services.AddScoped<IParticipantService, ParticipantService>();
        services.AddScoped<IClaimQueryService, ClaimQueryService>();
        services.AddScoped<IClaimCommandService, ClaimCommandService>();
        services.AddScoped<IClaimReviewService, ClaimReviewService>();
        services.AddScoped<IAuditTrailService, AuditTrailService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IClaimRiskFeatureBuilder, ClaimRiskFeatureBuilder>();
        services.AddScoped<IClaimRiskAssessmentService, ClaimRiskAssessmentService>();
        services.AddScoped<IServiceProviderEmployeeService, ServiceProviderEmployeeService>();

        services.AddScoped< IParticipantServiceService, ParticipantServiceService>();

        services.AddScoped<IParticipantBudgetPlanService, ParticipantBudgetPlanService>();
        services.AddScoped<IBudgetPlanTemplateService, BudgetPlanTemplateService>();
        services.AddScoped<IClaimDecisionSupportQueryService,ClaimDecisionSupportQueryService>();
        return services;
    }
}
