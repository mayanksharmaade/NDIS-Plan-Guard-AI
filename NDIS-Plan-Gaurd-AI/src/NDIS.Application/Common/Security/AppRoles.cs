namespace NDIS.Application.Common.Security;

public static class AppRoles
{
    public const string SuperAdmin = "SuperAdmin";
    public const string Admin = "Admin";
    public const string Reviewer = "Reviewer";
    public const string ServiceProvider = "ServiceProvider";

    public const string AdminOrSuperAdmin = Admin + "," + SuperAdmin;
    public const string Reviewers = Reviewer + "," + Admin + "," + SuperAdmin;
    public const string ClaimReaders = ServiceProvider + "," + Admin + "," + Reviewer + "," + SuperAdmin;

    public static readonly string[] All =
    [
        SuperAdmin,
        Admin,
        Reviewer,
        ServiceProvider
    ];
}
