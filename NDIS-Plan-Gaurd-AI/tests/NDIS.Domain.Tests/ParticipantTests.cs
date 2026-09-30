using NDIS.Domain.Entities;
using static NDIS.Domain.Enums.Enums;

namespace NDIS.Domain.Tests;

public class ParticipantTests
{
    [Fact]
    public void New_participant_is_active()
    {
        var participant = new Participant(Guid.NewGuid(), "430000000", "Test", "Participant", null,null,null,null, null, null, null, null);
        Assert.Equal(ParticipantStatus.Active, participant.Status);
    }
}
