namespace NDIS.PlanGuard.RiskScoring.Reference;

public sealed class RiskScoringOptions
{
    public const string SectionName = "RiskScoring";
    public string BaseUrl { get; init; } = "http://localhost:8001";
    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(5);
}
