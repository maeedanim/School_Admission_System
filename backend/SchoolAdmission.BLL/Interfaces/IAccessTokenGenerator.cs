
using SchoolAdmission.BLL.Models.Auth;

namespace SchoolAdmission.BLL.Interfaces;

public interface IAccessTokenGenerator
{
    Task<(string Token, DateTime ExpiresAtUtc)> GenerateAsync(
        AuthUserData user,
        CancellationToken cancellationToken = default);
}
