
namespace SchoolAdmission.BLL.Models.Auth;

public enum AuthSessionStatus
{
    Succeeded,
    InvalidCredentials,
    AccountLocked,
    AccountInactive,
    RoleMismatch,
    InvalidRefreshToken
}
