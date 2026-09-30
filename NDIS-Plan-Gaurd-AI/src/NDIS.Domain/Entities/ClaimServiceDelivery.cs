namespace NDIS.Domain.Entities;

public sealed class ClaimServiceDelivery
{
    public Guid ClaimId { get; set; }
    public Guid ServiceDeliveryId { get; set; }

    public Claim Claim { get; set; } = null!;
    public ServiceDelivery ServiceDelivery { get; set; } = null!;
}
