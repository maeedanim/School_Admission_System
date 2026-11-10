
namespace SchoolAdmission.BLL.Models.Auth;

public class AuthUserData
{
    public Guid Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? BirthRegistrationNumber { get; set; }

    public bool IsActive { get; set; }

    public IReadOnlyList<string> Roles { get; set; }
        = Array.Empty<string>();
}
