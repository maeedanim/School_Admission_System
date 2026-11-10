
namespace SchoolAdmission.BLL.Models.Auth;

public class AuthenticationAttemptResult
{
    public AuthenticationAttemptStatus Status { get; set; }

    public AuthUserData? User { get; set; }
}
