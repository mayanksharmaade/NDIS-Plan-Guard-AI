namespace NDIS.Domain.Enums;

public class Enums
{
    public enum ApprovalStatus
    {
        Pending = 1,
        Approved = 2,
        Rejected = 3
    }

    public enum AccountStatus
    {
        Inactive = 1,
        Active = 2,
        Suspended = 3,
        Disabled = 4
    }

    public enum UserCategory
    {
        Internal = 1,
        ServiceProvider = 2
    }

    public enum ServiceProviderStatus
    {
        Inactive = 1,
        Active = 2,
        Suspended = 3,
        Closed = 4
    }

    public enum ProviderMembershipStatus
    {
        Pending = 1,
        Active = 2,
        Suspended = 3,
        Removed = 4
    }

    public enum ParticipantStatus
    {
        Active = 1,
        Inactive = 2,
        Archived = 3
    }

    public enum ClaimStatus
    {
        Draft = 1,
        Submitted = 2,
        Processing = 3,
        ReviewRequired = 4,
        Approved = 5,
        Rejected = 6,
        MoreInformationRequired = 7,
        Cancelled = 8
    }

    public enum ClaimReviewOutcome
    {
        Approved = 1,
        Rejected = 2,
        MoreInformationRequired = 3
    }

    public enum AuditActionType
    {
        ClaimCreated = 1,
        ClaimUpdated = 2,
        ClaimSubmitted = 3,
        ClaimReviewStarted = 4,
        ClaimApproved = 5,
        ClaimRejected = 6,
        ClaimMoreInformationRequested = 7,
        ClaimRiskScored = 8,
        ClaimRiskScoringFailed = 9
    }
    public enum EmployeeRole
    {
        SupportWorker = 1,
        RegisteredNurse = 2,
        Therapist = 3,
        Coordinator = 4,
        Other = 99
    }
    public enum BudgetPlanStatus
    {
        Draft = 1,
        Active = 2,
        Expired = 3,
        Cancelled = 4
    }

    public enum BudgetType
    {
        TotalPlan = 1,
        Fortnightly = 2
    }


}

