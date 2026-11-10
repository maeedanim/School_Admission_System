
namespace SchoolAdmission.BLL.Models.Auth;

public class IdentityOperationResult
{
    public bool Succeeded { get; set; }

    public bool BirthRegistrationNumberAlreadyExists { get; set; }

    public Guid? UserId { get; set; }

    public IReadOnlyList<string> Errors { get; set; }
        = Array.Empty<string>();
}
