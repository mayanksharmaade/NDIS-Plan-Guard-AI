namespace NDIS.Application.Abstractions.Identity;

public sealed record IdentityUserCreationResult(
    bool Succeeded,
    Guid? UserId,
    IReadOnlyCollection<string> Errors)
{
    public static IdentityUserCreationResult Success(Guid userId)
        => new(true, userId, Array.Empty<string>());

    public static IdentityUserCreationResult Failure(
        IEnumerable<string> errors)
        => new(false, null, errors.ToArray());
}
