using SchoolAdmission.BLL.Enums;

namespace SchoolAdmission.BLL.Models;

public class MyHoldData
{
    public Guid HoldId { get; set; }

    public Guid ClassId { get; set; }

    public string ClassName { get; set; } = string.Empty;

    public AdmissionHoldStatus Status { get; set; }

    public DateTime ExpiresAtUtc { get; set; }
}