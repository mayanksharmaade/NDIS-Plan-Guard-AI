using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NDIS.Application.Common.Security;
using NDIS.Domain.Entities;
using NDIS.Infrastructure.Persistence;
using static NDIS.Domain.Enums.Enums;

namespace NDIS.Infrastructure.Identity;

public static class SuperAdminSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var configuration =
            serviceProvider.GetRequiredService<IConfiguration>();

        if (!bool.TryParse(
                configuration["BootstrapSuperAdmin:Enabled"],
                out var enabled) ||
            !enabled)
        {
            return;
        }

        var email = configuration["BootstrapSuperAdmin:Email"]
            ?.Trim()
            .ToLowerInvariant();

        var password =
            configuration["BootstrapSuperAdmin:Password"];

        var firstName =
            configuration["BootstrapSuperAdmin:FirstName"]
            ?? "System";

        var lastName =
            configuration["BootstrapSuperAdmin:LastName"]
            ?? "Administrator";

        var resetPassword =
            bool.TryParse(
                configuration["BootstrapSuperAdmin:ResetPassword"],
                out var shouldResetPassword)
            && shouldResetPassword;

        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException(
                "BootstrapSuperAdmin email/password must be configured when enabled.");
        }

        var userManager =
            serviceProvider.GetRequiredService<
                UserManager<ApplicationUser>>();

        var dbContext =
            serviceProvider.GetRequiredService<AppDbContext>();

        var user =
            await userManager.FindByEmailAsync(email);

        // --------------------------------------------------
        // Create SuperAdmin identity user if it does not exist
        // --------------------------------------------------

        if (user is null)
        {
            user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                LockoutEnabled = true
            };

            var createResult =
                await userManager.CreateAsync(
                    user,
                    password);

            if (!createResult.Succeeded)
            {
                throw new InvalidOperationException(
                    string.Join(
                        "; ",
                        createResult.Errors.Select(
                            x => x.Description)));
            }
        }

        // --------------------------------------------------
        // Optional ONE-TIME password reset
        // --------------------------------------------------

        else if (resetPassword)
        {
            var resetToken =
                await userManager
                    .GeneratePasswordResetTokenAsync(user);

            var resetResult =
                await userManager.ResetPasswordAsync(
                    user,
                    resetToken,
                    password);

            if (!resetResult.Succeeded)
            {
                throw new InvalidOperationException(
                    "Unable to reset bootstrap SuperAdmin password: " +
                    string.Join(
                        "; ",
                        resetResult.Errors.Select(
                            x => x.Description)));
            }
        }

        // --------------------------------------------------
        // Ensure SuperAdmin role
        // --------------------------------------------------

        if (!await userManager.IsInRoleAsync(
                user,
                AppRoles.SuperAdmin))
        {
            var roleResult =
                await userManager.AddToRoleAsync(
                    user,
                    AppRoles.SuperAdmin);

            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException(
                    string.Join(
                        "; ",
                        roleResult.Errors.Select(
                            x => x.Description)));
            }
        }

        // --------------------------------------------------
        // Ensure UserProfile exists and is active
        // --------------------------------------------------

        var profile =
            await dbContext.UserProfiles
                .SingleOrDefaultAsync(
                    x => x.IdentityUserId == user.Id);

        if (profile is null)
        {
            profile = new UserProfile(
                user.Id,
                firstName,
                lastName,
                UserCategory.Internal);

            profile.Approve();

            dbContext.UserProfiles.Add(profile);

            await dbContext.SaveChangesAsync();
        }
        else if (
            profile.ApprovalStatus != ApprovalStatus.Approved ||
            profile.AccountStatus != AccountStatus.Active)
        {
            profile.Approve();

            await dbContext.SaveChangesAsync();
        }
    }
}