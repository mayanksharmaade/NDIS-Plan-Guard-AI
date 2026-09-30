using System;
using System.Collections.Generic;
using System.Text;

namespace NDIS.Application.Reviews.Models
{
    public sealed record ReviewHistoryItemDto
    {
        public Guid Id { get; init; }

        public required string Decision { get; init; }

        public string? ReviewerName { get; init; }

        public string? ReviewerEmail { get; init; }

        public string? Comments { get; init; }

        public DateTimeOffset ReviewedAtUtc { get; init; }
    }
}
