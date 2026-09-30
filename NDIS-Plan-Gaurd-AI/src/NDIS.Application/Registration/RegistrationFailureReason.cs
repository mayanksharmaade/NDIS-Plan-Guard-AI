namespace NDIS.Application.Registration;

public enum RegistrationFailureReason
{
    None = 0,
    Validation = 1,
    DuplicateEmail = 2,
    DuplicateAbn = 3,
    IdentityCreationFailed = 4,
    RoleAssignmentFailed = 5
}
