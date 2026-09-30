namespace NDIS.Domain.Common;

public static class SupportServiceCatalog
{
    public sealed record SupportServiceOption(string Code, string Name);

    public static readonly IReadOnlyCollection<SupportServiceOption> All =
    [
        new("PERSONAL_CARE", "Personal Care Support"),
        new("DAILY_LIVING", "Assistance with Daily Living"),
        new("HOUSEHOLD_TASKS", "Household Tasks"),
        new("MEAL_PREPARATION", "Meal Preparation"),
        new("COMMUNITY_ACCESS", "Community Access"),
        new("SOCIAL_PARTICIPATION", "Social & Community Participation"),
        new("GROUP_ACTIVITIES", "Group Activities"),
        new("TRANSPORT_ASSISTANCE", "Transport Assistance"),
        new("LIFE_SKILLS", "Life Skills Development"),
        new("SUPPORT_COORDINATION", "Support Coordination"),
        new("RECOVERY_COACHING", "Psychosocial Recovery Coaching"),
        new("OCCUPATIONAL_THERAPY", "Occupational Therapy"),
        new("PHYSIOTHERAPY", "Physiotherapy"),
        new("SPEECH_PATHOLOGY", "Speech Pathology"),
        new("PSYCHOLOGY", "Psychology Support"),
        new("BEHAVIOUR_SUPPORT", "Behaviour Support"),
        new("COMMUNITY_NURSING", "Community Nursing Care"),
        new("EMPLOYMENT_SUPPORT", "Employment Support")
    ];

    public static string Normalize(string code) => code.Trim().ToUpperInvariant();

    public static bool IsValid(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return false;

        var normalized = Normalize(code);
        return All.Any(x => x.Code == normalized);
    }

    public static string GetName(string code)
    {
        var normalized = Normalize(code);
        return All.FirstOrDefault(x => x.Code == normalized)?.Name ?? normalized;
    }
}
