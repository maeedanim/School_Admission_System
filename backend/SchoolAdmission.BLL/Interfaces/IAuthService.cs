
using SchoolAdmission.BLL.Models.Auth;

namespace SchoolAdmission.BLL.Interfaces;

public interface IAuthService
{
    Task<RegistrationResult> RegisterStudentAsync(
        string fullName,
        string email,
        string password,
        string birthRegistrationNumber,
        CancellationToken cancellationToken = default);

    Task<AuthSessionResult> LoginStudentAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default);

    Task<AuthSessionResult> RefreshSessionAsync(
        string refreshToken,
        CancellationToken cancellationToken = default);

    Task LogoutAsync(
        string refreshToken,
        CancellationToken cancellationToken = default);

    Task<AuthUserData?> GetCurrentUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
