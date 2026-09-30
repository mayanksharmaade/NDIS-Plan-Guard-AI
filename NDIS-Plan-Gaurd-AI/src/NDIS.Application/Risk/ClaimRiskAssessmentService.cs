using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using NDIS.Application.Abstractions.Persistence;
using NDIS.Application.Abstractions.Risk;
using NDIS.Application.Auditing;
using NDIS.Domain.Entities;
using static NDIS.Domain.Enums.Enums;

namespace NDIS.Application.Risk;

public sealed class ClaimRiskAssessmentService : IClaimRiskAssessmentService
{
    private readonly IAppDbContext _dbContext;
    private readonly IClaimRiskFeatureBuilder _featureBuilder;
    private readonly IRiskScoringService _riskScoringService;
    private readonly IAuditTrailService _auditTrail;

    public ClaimRiskAssessmentService(
        IAppDbContext dbContext,
        IClaimRiskFeatureBuilder featureBuilder,
        IRiskScoringService riskScoringService,
        IAuditTrailService auditTrail)
    {
        _dbContext = dbContext;
        _featureBuilder = featureBuilder;
        _riskScoringService = riskScoringService;
        _auditTrail = auditTrail;
    }

    public async Task<ClaimRiskAssessmentDto> ScoreAndPersistAsync(
        Guid claimId,
        string? correlationId = null,
        CancellationToken cancellationToken = default)
    {
        var claimInfo = await _dbContext.Claims
            .AsNoTracking()
            .Where(x => x.Id == claimId)
            .Select(x => new { x.Id, x.ClaimNumber, x.ServiceProviderId })
            .SingleOrDefaultAsync(cancellationToken);

        if (claimInfo is null)
            throw new InvalidOperationException("Claim was not found for ML risk scoring.");

        var build = await _featureBuilder.BuildAsync(claimId, correlationId, cancellationToken);

        RiskAssessmentResult result;

        if (!build.Succeeded || build.Context is null)
        {
            result = RiskAssessmentResult.Unavailable(
                build.Error ?? "ML risk features could not be built.");
        }
        else
        {
            try
            {
                result = await _riskScoringService.ScoreAsync(build.Context, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch
            {
                result = RiskAssessmentResult.Unavailable(
                    "ML risk scoring failed unexpectedly. Human review can continue without an ML score.");
            }
        }

        var entity = new ClaimRiskAssessment(
            claimId,
            result.Available,
            result.Probability,
            result.Score,
            result.RiskBand,
            result.ModelVersion,
            result.FeatureVersion,
            result.ScoredAtUtc,
            result.FailureReason);

        foreach (var factor in result.Factors)
        {
            entity.AddFactor(
                factor.Feature,
                factor.Description,
                SerializeValue(factor.ObservedValue),
                SerializeValue(factor.BaselineValue),
                factor.ProbabilityImpact);
        }

        _dbContext.ClaimRiskAssessments.Add(entity);

        _auditTrail.Add(new AddAuditEventCommand(
            claimInfo.ServiceProviderId,
            "Claim",
            claimInfo.Id,
            result.Available ? AuditActionType.ClaimRiskScored : AuditActionType.ClaimRiskScoringFailed,
            result.Available
                ? $"ML risk assessment completed for claim {claimInfo.ClaimNumber}: {result.RiskBand} ({result.Score}/100), model {result.ModelVersion}."
                : $"ML risk assessment was unavailable for claim {claimInfo.ClaimNumber}: {result.FailureReason}"));

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Map(entity);
    }

    public async Task<ClaimRiskAssessmentDto?> GetLatestAsync(
        Guid claimId,
        CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.ClaimRiskAssessments
            .AsNoTracking()
            .Include(x => x.Factors)
            .Where(x => x.ClaimId == claimId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        return entity is null ? null : Map(entity);
    }

    private static ClaimRiskAssessmentDto Map(ClaimRiskAssessment entity) =>
        new(
            entity.Id,
            entity.ClaimId,
            entity.Available,
            entity.Probability,
            entity.Score,
            entity.RiskBand,
            entity.ModelVersion,
            entity.FeatureVersion,
            entity.ScoredAtUtc,
            entity.FailureReason,
            entity.CreatedAtUtc,
            entity.Factors
                .OrderByDescending(x => Math.Abs(x.ProbabilityImpact))
                .Select(x => new ClaimRiskFactorDto(
                    x.Feature,
                    x.Description,
                    DeserializeValue(x.ObservedValueJson),
                    DeserializeValue(x.BaselineValueJson),
                    x.ProbabilityImpact))
                .ToArray());

    private static string? SerializeValue(object? value) =>
        value is null ? null : JsonSerializer.Serialize(value);

    private static object? DeserializeValue(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        try
        {
            return JsonSerializer.Deserialize<object>(value);
        }
        catch (JsonException)
        {
            return value;
        }
    }
}
