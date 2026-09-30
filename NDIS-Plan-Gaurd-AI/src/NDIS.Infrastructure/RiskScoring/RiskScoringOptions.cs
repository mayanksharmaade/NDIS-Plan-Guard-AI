using System;
using System.Collections.Generic;
using System.Text;

namespace NDIS.Infrastructure.RiskScoring;

public sealed class RiskScoringOptions
{
    public const string SectionName = "RiskScoring";

    public string BaseUrl { get; init; }
        = "http://localhost:8001";

    public int TimeoutSeconds { get; init; } = 5;
}
