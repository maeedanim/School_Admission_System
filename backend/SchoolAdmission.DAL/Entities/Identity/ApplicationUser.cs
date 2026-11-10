
using Microsoft.AspNetCore.Identity;

namespace SchoolAdmission.DAL.Entities.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
    public string FullName { get; set; } = string.Empty;

    public string? BirthRegistrationNumber { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; }

    public ICollection<RefreshToken> RefreshTokens { get; set; }
        = new List<RefreshToken>();

    public ICollection<AdmissionHold> Holds { get; set; }
        = new List<AdmissionHold>();
}
