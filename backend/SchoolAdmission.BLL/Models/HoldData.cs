using SchoolAdmission.BLL.Enums;

namespace SchoolAdmission.BLL.Models;

public class HoldData
{
    public Guid Id { get; set; }

    public Guid ClassId { get; set; }

    public string StudentId { get; set; } = string.Empty;

    public AdmissionHoldStatus Status { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime ExpiresAtUtc { get; set; }

    public DateTime? ConfirmedAtUtc { get; set; }

    public DateTime? CancelledAtUtc { get; set; }
}