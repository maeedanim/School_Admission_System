using SchoolAdmission.BLL.Enums;
using SchoolAdmission.DAL.Entities.Identity;

namespace SchoolAdmission.DAL.Entities;

public class AdmissionHold
{
    public Guid Id { get; set; }

    public Guid ClassId { get; set; }

    public string StudentId { get; set; } = string.Empty;

    public AdmissionHoldStatus Status { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime ExpiresAtUtc { get; set; }

    public DateTime? ConfirmedAtUtc { get; set; }

    public DateTime? CancelledAtUtc { get; set; }
    public Guid? UserId { get; set; }

    public ApplicationUser? User { get; set; }

    public AdmissionClass Class { get; set; } = null!;
}