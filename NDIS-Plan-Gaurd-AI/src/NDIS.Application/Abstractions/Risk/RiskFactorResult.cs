using System;
using System.Collections.Generic;
using System.Text;

namespace NDIS.Application.Abstractions.Risk;

public sealed record RiskFactorResult
{
    public required string Feature { get; init; }

    public required string Description { get; init; }

    public object? ObservedValue { get; init; }

    public object? BaselineValue { get; init; }

    public double ProbabilityImpact { get; init; }
}
