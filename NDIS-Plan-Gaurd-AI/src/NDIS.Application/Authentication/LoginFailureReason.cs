namespace NDIS.Application.Authentication;

public enum LoginFailureReason
{
    None = 0,
    InvalidCredentials = 1,
    PendingApproval = 2,
    Rejected = 3,
    AccountInactive = 4
}
