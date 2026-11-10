
using SchoolAdmission.BLL.Models.Auth;

namespace SchoolAdmission.BLL.Interfaces;

public interface IIdentityRepository
{
    Task<AuthUserData?> FindByEmailAsync(
        string email,
        CancellationToken cancellationToken = default);

    Task<bool> BirthRegistrationNumberExistsAsync(
    string birthRegistrationNumber,
    CancellationToken cancellationToken = default);

    Task<AuthUserData?> FindByIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<IdentityOperationResult> CreateStudentAsync(
        string fullName,
        string email,
        string password,
        string birthRegistrationNumber,
        CancellationToken cancellationToken = default);

    Task<AuthenticationAttemptResult> AuthenticateAsync(
        string email,
        string password,
        string requiredRole,
        CancellationToken cancellationToken = default);
}
