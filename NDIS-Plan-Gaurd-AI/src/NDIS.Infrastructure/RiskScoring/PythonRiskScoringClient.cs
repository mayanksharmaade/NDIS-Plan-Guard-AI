using System;
using System.Collections.Generic;
using System.Text;

using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

namespace NDIS.Infrastructure.RiskScoring;

public sealed class PythonRiskScoringClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<PythonRiskScoringClient> _logger;

    public PythonRiskScoringClient(
        HttpClient httpClient,
        ILogger<PythonRiskScoringClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<PythonRiskScoreResponse?> ScoreAsync(
        PythonRiskScoreRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var response =
                await _httpClient.PostAsJsonAsync(
                    "/api/v1/risk/score",
                    request,
                    cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

                _logger.LogWarning(
                    "Python risk service returned HTTP {StatusCode} for claim {ClaimId}. Response: {ResponseBody}",
                    (int)response.StatusCode,
                    request.ClaimId,
                    responseBody);

                return null;
            }

            var result =
                await response.Content
                    .ReadFromJsonAsync<PythonRiskScoreResponse>(
                        cancellationToken: cancellationToken);

            if (result is null)
            {
                _logger.LogWarning(
                    "Python risk service returned an empty response for claim {ClaimId}.",
                    request.ClaimId);

                return null;
            }

            return result;
        }
        catch (OperationCanceledException)
            when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning(
                "Python risk scoring timed out for claim {ClaimId}.",
                request.ClaimId);

            return null;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(
                ex,
                "Python risk service is unavailable for claim {ClaimId}.",
                request.ClaimId);

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected error calling Python risk service for claim {ClaimId}.",
                request.ClaimId);

            return null;
        }
    }
}
