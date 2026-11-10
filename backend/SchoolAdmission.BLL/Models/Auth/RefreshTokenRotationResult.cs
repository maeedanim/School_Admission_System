
namespace SchoolAdmission.BLL.Models.Auth;

public class RefreshTokenRotationResult
{
    public Guid UserId { get; set; }

    public string RefreshToken { get; set; } = string.Empty;

    public DateTime ExpiresAtUtc { get; set; }
}
