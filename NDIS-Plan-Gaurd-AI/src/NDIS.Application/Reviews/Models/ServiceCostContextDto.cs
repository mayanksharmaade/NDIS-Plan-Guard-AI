namespace NDIS.Application.Reviews.Models;

public sealed record ServiceCostContextDto
{
    public string? EmployeeName { get; init; }
    public int DeliveryCount { get; init; }
    public decimal? EmployeePayRate { get; init; }
    public decimal? ServiceHours { get; init; }
    public decimal? ClaimedHourlyRate { get; init; }
    public decimal? ProviderTypicalHourlyRate { get; init; }
    public decimal? ServiceTypicalHourlyRate { get; init; }
    public decimal? ExpectedServiceCost { get; init; }
    public decimal ClaimedAmount { get; init; }
    public decimal? AmountVariance { get; init; }
    public decimal? RateVariancePercent { get; init; }
    public string? ServiceLocation { get; init; }
    public int ConcurrentParticipantCount { get; init; }
    public int OverlappingServiceMinutes { get; init; }
}
