
using SchoolAdmission.BLL.Models.Auth;

namespace SchoolAdmission.BLL.Interfaces;

public interface IRefreshTokenRepository
{
    Task<(string Token, DateTime ExpiresAtUtc)> CreateAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<RefreshTokenRotationResult?> RotateAsync(
        string rawToken,
        CancellationToken cancellationToken = default);

    Task RevokeAsync(
        string rawToken,
        CancellationToken cancellationToken = default);
}
