
namespace SchoolAdmission.BLL.Models.Auth;

public class AuthSessionData
{
    public AuthUserData User { get; set; } = null!;

    public string AccessToken { get; set; } = string.Empty;

    public DateTime AccessTokenExpiresAtUtc { get; set; }

    public string RefreshToken { get; set; } = string.Empty;

    public DateTime RefreshTokenExpiresAtUtc { get; set; }
}
