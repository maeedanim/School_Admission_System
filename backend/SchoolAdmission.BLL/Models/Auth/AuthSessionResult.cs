
namespace SchoolAdmission.BLL.Models.Auth;

public class AuthSessionResult
{
    public AuthSessionStatus Status { get; set; }

    public AuthSessionData? Session { get; set; }
}
