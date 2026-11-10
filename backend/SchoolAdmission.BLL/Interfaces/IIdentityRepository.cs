
using SchoolAdmission.BLL.Models.Auth;

namespace SchoolAdmission.BLL.Interfaces;

public interface IIdentityRepository
{
    // User lookup
    Task<AuthUserData?> FindByEmailAsync(
        string email,
        CancellationToken cancellationToken = default);

    Task<AuthUserData?> FindByIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    // Registration number validation
    Task<bool> BirthRegistrationNumberExistsAsync(
        string birthRegistrationNumber,
        CancellationToken cancellationToken = default);

    // User creation
    Task<IdentityOperationResult> CreateStudentAsync(
        string fullName,
        string email,
        string password,
        string birthRegistrationNumber,
        CancellationToken cancellationToken = default);

    Task<IdentityOperationResult> CreateAdminAsync(
        string fullName,
        string email,
        string password,
        CancellationToken cancellationToken = default);

    // Authentication
    Task<AuthenticationAttemptResult> AuthenticateAsync(
        string email,
        string password,
        string requiredRole,
        CancellationToken cancellationToken = default);
}
