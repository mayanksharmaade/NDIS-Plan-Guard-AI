using NDIS.Application.Reviews.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace NDIS.Application.Reviews
{
    public interface IClaimDecisionSupportQueryService
    {
        Task<ClaimDecisionSupportDto?> GetAsync(
            Guid claimId,
            CancellationToken cancellationToken = default);
    }
}
