using System;
using System.Collections.Generic;
using System.Text;

namespace NDIS.Application.Reviews.Models
{
    public sealed record DecisionSupportFindingDto
    {
        public required string Code { get; init; }

        public required string Title { get; init; }

        public required string Description { get; init; }

        public required string Severity { get; init; }

        public string? Evidence { get; init; }
    }
}
