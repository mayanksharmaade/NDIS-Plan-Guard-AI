using System;
using System.Collections.Generic;
using System.Text;

namespace NDIS.Application.Reviews.Models
{
    public sealed record ParticipantSupportContextDto
    {
        public required string ParticipantName { get; init; }

        public required string NdisNumber { get; init; }

        public string? PrimaryDisabilityCategory { get; init; }

        public IReadOnlyCollection<string> FunctionalSupportDomains { get; init; }
            = Array.Empty<string>();

        public IReadOnlyCollection<string> ApprovedSupportCategories { get; init; }
            = Array.Empty<string>();

        public decimal? TypicalSupportHoursPerFortnight { get; init; }

        public string? SupportIntensity { get; init; }

        public string? SpecialSupportRequirements { get; init; }
    }
}
