using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

namespace NDIS.PlanGuard.RiskScoring.Reference;

/// <summary>
/// Reference Infrastructure adapter only. In the real NDIS solution, implement the existing
/// Application risk-scoring abstraction and map this transport response into the Application contract.
/// </summary>
public sealed class PythonRiskScoringClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<PythonRiskScoringClient> _logger;

    public PythonRiskScoringClient(HttpClient httpClient, ILogger<PythonRiskScoringClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<PythonRiskScoreResponse?> ScoreAsync(
        PythonRiskScoreRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync(
            "/api/v1/risk/score",
            request,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "Python risk service returned {StatusCode} for claim {ClaimId}",
                response.StatusCode,
                request.ClaimId);
            return null;
        }

        return await response.Content.ReadFromJsonAsync<PythonRiskScoreResponse>(
            cancellationToken: cancellationToken);
    }
}
