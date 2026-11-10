
namespace SchoolAdmission.BLL.Models.Auth;

public enum AuthenticationAttemptStatus
{
    Succeeded,
    InvalidCredentials,
    AccountLocked,
    AccountInactive,
    RoleMismatch
}
