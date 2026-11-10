
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;

using SchoolAdmission.BLL.Interfaces;
using SchoolAdmission.BLL.Models.Auth;
using SchoolAdmission.DAL.Data;
using SchoolAdmission.DAL.Entities.Identity;

namespace SchoolAdmission.DAL.Repositories;

public class IdentityRepository : IIdentityRepository
{
    private const string StudentRole = "Student";

    private const string BirthRegistrationNumberConstraint =
        "IX_AspNetUsers_BirthRegistrationNumber";

    private readonly SchoolAdmissionDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public IdentityRepository(
        SchoolAdmissionDbContext context,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _context = context;
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task<AuthUserData?> FindByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = await _userManager.FindByEmailAsync(email);

        return user is null
            ? null
            : await MapUserAsync(user);
    }

    public async Task<bool> BirthRegistrationNumberExistsAsync(
        string birthRegistrationNumber,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return await _context.Users
            .AsNoTracking()
            .AnyAsync(
                user => user.BirthRegistrationNumber == birthRegistrationNumber,
                cancellationToken);
    }

    public async Task<AuthUserData?> FindByIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = await _userManager.FindByIdAsync(userId.ToString());

        return user is null
            ? null
            : await MapUserAsync(user);
    }

    public async Task<IdentityOperationResult> CreateStudentAsync(
        string fullName,
        string email,
        string password,
        string birthRegistrationNumber,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        await using var transaction =
            await _context.Database.BeginTransactionAsync(cancellationToken);

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FullName = fullName,
            BirthRegistrationNumber = birthRegistrationNumber,
            IsActive = true,
            EmailConfirmed = false,
            CreatedAtUtc = DateTime.UtcNow
        };

        IdentityResult createResult;

        try
        {
            createResult = await _userManager.CreateAsync(user, password);
        }
        catch (DbUpdateException exception)
            when (IsDuplicateBirthRegistrationNumber(exception))
        {
            await transaction.RollbackAsync(cancellationToken);

            return new IdentityOperationResult
            {
                Succeeded = false,
                BirthRegistrationNumberAlreadyExists = true,
                Errors = new[]
                {
                    "An account with this birth registration number already exists."
                }
            };
        }

        if (!createResult.Succeeded)
        {
            await transaction.RollbackAsync(cancellationToken);

            return new IdentityOperationResult
            {
                Succeeded = false,
                Errors = createResult.Errors
                    .Select(error => error.Description)
                    .ToArray()
            };
        }

        var roleResult =
            await _userManager.AddToRoleAsync(user, StudentRole);

        if (!roleResult.Succeeded)
        {
            await transaction.RollbackAsync(cancellationToken);

            return new IdentityOperationResult
            {
                Succeeded = false,
                Errors = roleResult.Errors
                    .Select(error => error.Description)
                    .ToArray()
            };
        }

        await transaction.CommitAsync(cancellationToken);

        return new IdentityOperationResult
        {
            Succeeded = true,
            UserId = user.Id
        };
    }

    public async Task<AuthenticationAttemptResult> AuthenticateAsync(
        string email,
        string password,
        string requiredRole,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = await _userManager.FindByEmailAsync(email);

        if (user is null)
        {
            return new AuthenticationAttemptResult
            {
                Status = AuthenticationAttemptStatus.InvalidCredentials
            };
        }

        var signInResult =
            await _signInManager.CheckPasswordSignInAsync(
                user,
                password,
                lockoutOnFailure: true);

        if (signInResult.IsLockedOut)
        {
            return new AuthenticationAttemptResult
            {
                Status = AuthenticationAttemptStatus.AccountLocked
            };
        }

        if (!signInResult.Succeeded)
        {
            return new AuthenticationAttemptResult
            {
                Status = AuthenticationAttemptStatus.InvalidCredentials
            };
        }

        if (!user.IsActive)
        {
            return new AuthenticationAttemptResult
            {
                Status = AuthenticationAttemptStatus.AccountInactive
            };
        }

        var hasRequiredRole =
            await _userManager.IsInRoleAsync(user, requiredRole);

        if (!hasRequiredRole)
        {
            return new AuthenticationAttemptResult
            {
                Status = AuthenticationAttemptStatus.RoleMismatch
            };
        }

        return new AuthenticationAttemptResult
        {
            Status = AuthenticationAttemptStatus.Succeeded,
            User = await MapUserAsync(user)
        };
    }

    private async Task<AuthUserData> MapUserAsync(
        ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);

        return new AuthUserData
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            BirthRegistrationNumber = user.BirthRegistrationNumber,
            IsActive = user.IsActive,
            Roles = roles.ToArray()
        };
    }

    private static bool IsDuplicateBirthRegistrationNumber(
        DbUpdateException exception)
    {
        return exception.GetBaseException() is PostgresException postgresException
            && postgresException.SqlState == PostgresErrorCodes.UniqueViolation
            && postgresException.ConstraintName ==
               BirthRegistrationNumberConstraint;
    }
}
