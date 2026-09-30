using System.Net.Mail;
using Microsoft.EntityFrameworkCore;
using NDIS.Application.Abstractions.Identity;
using NDIS.Application.Abstractions.Persistence;
using NDIS.Application.Common.Security;
using NDIS.Application.Common.Validation;
using NDIS.Domain.Entities;
using static NDIS.Domain.Enums.Enums;

namespace NDIS.Application.Registration;

public sealed class RegistrationService : IRegistrationService
{
    private readonly IIdentityService _identityService;
    private readonly IAppDbContext _dbContext;
    private readonly ITransactionManager _transactionManager;

    public RegistrationService(
        IIdentityService identityService,
        IAppDbContext dbContext,
        ITransactionManager transactionManager)
    {
        _identityService = identityService;
        _dbContext = dbContext;
        _transactionManager = transactionManager;
    }

    public async Task<RegisterServiceProviderResult>
        RegisterServiceProviderAsync(
            RegisterServiceProviderCommand command,
            CancellationToken cancellationToken = default)
    {
        var validationErrors = Validate(command);

        if (validationErrors.Count != 0)
        {
            return RegisterServiceProviderResult.Failure(
                RegistrationFailureReason.Validation,
                validationErrors.ToArray());
        }

        var normalizedEmail = command.Email
            .Trim()
            .ToLowerInvariant();

        var normalizedAbn =
            AbnValidator.Normalize(command.Abn);

        if (await _identityService.EmailExistsAsync(
                normalizedEmail,
                cancellationToken))
        {
            return RegisterServiceProviderResult.Failure(
                RegistrationFailureReason.DuplicateEmail,
                "A user with this email address already exists.");
        }

        var abnExists = await _dbContext.ServiceProviders
            .AnyAsync(
                x => x.Abn == normalizedAbn,
                cancellationToken);

        if (abnExists)
        {
            return RegisterServiceProviderResult.Failure(
                RegistrationFailureReason.DuplicateAbn,
                "A service provider with this ABN already exists.");
        }

        try
        {
            return await _transactionManager.ExecuteAsync(
                async token =>
                {
                    var identityResult =
                        await _identityService.CreateUserAsync(
                            normalizedEmail,
                            command.Password,
                            command.PhoneNumber,
                            token);

                    if (!identityResult.Succeeded ||
                        identityResult.UserId is null)
                    {
                        return RegisterServiceProviderResult.Failure(
                            RegistrationFailureReason.IdentityCreationFailed,
                            identityResult.Errors.ToArray());
                    }

                    var identityUserId =
                        identityResult.UserId.Value;

                    var roleResult =
                        await _identityService.AddToRoleAsync(
                            identityUserId,
                            AppRoles.ServiceProvider,
                            token);

                    if (!roleResult.Succeeded)
                    {
                        throw new RegistrationWorkflowException(
                            RegistrationFailureReason.RoleAssignmentFailed,
                            roleResult.Errors);
                    }

                    var userProfile = new UserProfile(
                        identityUserId,
                        command.FirstName.Trim(),
                        command.LastName.Trim(),
                        UserCategory.ServiceProvider);

                    var serviceProvider =
                        new ServiceProvider(
                            command.LegalName.Trim(),
                            NullIfWhiteSpace(command.TradingName),
                            normalizedAbn,
                            NullIfWhiteSpace(
                                command.NdisRegistrationNumber));

                    var membership =
                        new ServiceProviderUser(
                            serviceProvider.Id,
                            userProfile.Id,
                            isPrimaryContact: true);

                    _dbContext.UserProfiles.Add(userProfile);
                    _dbContext.ServiceProviders.Add(serviceProvider);
                    _dbContext.ServiceProviderUsers.Add(membership);

                    await _dbContext.SaveChangesAsync(token);

                    return RegisterServiceProviderResult.Success(
                        userProfile.Id,
                        serviceProvider.Id);
                },
                cancellationToken);
        }
        catch (RegistrationWorkflowException exception)
        {
            return RegisterServiceProviderResult.Failure(
                exception.Reason,
                exception.Errors.ToArray());
        }
    }

    private static List<string> Validate(
        RegisterServiceProviderCommand command)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(command.Email) ||
            !MailAddress.TryCreate(
                command.Email,
                out _))
        {
            errors.Add("A valid email address is required.");
        }

        if (string.IsNullOrWhiteSpace(command.Password))
        {
            errors.Add("Password is required.");
        }

        if (string.IsNullOrWhiteSpace(command.FirstName))
        {
            errors.Add("First name is required.");
        }
        else if (command.FirstName.Length > 100)
        {
            errors.Add(
                "First name cannot exceed 100 characters.");
        }

        if (string.IsNullOrWhiteSpace(command.LastName))
        {
            errors.Add("Last name is required.");
        }
        else if (command.LastName.Length > 100)
        {
            errors.Add(
                "Last name cannot exceed 100 characters.");
        }

        if (string.IsNullOrWhiteSpace(command.LegalName))
        {
            errors.Add("Legal name is required.");
        }
        else if (command.LegalName.Length > 200)
        {
            errors.Add(
                "Legal name cannot exceed 200 characters.");
        }

        if (!string.IsNullOrWhiteSpace(command.TradingName) &&
            command.TradingName.Length > 200)
        {
            errors.Add(
                "Trading name cannot exceed 200 characters.");
        }

        if (!AbnValidator.IsValid(command.Abn))
        {
            errors.Add("A valid Australian ABN is required.");
        }

        if (!string.IsNullOrWhiteSpace(
                command.NdisRegistrationNumber) &&
            command.NdisRegistrationNumber.Length > 100)
        {
            errors.Add(
                "NDIS registration number cannot exceed 100 characters.");
        }

        return errors;
    }

    private static string? NullIfWhiteSpace(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

    private sealed class RegistrationWorkflowException
        : Exception
    {
        public RegistrationWorkflowException(
            RegistrationFailureReason reason,
            IEnumerable<string> errors)
        {
            Reason = reason;
            Errors = errors.ToArray();
        }

        public RegistrationFailureReason Reason { get; }

        public IReadOnlyCollection<string> Errors { get; }
    }
}
