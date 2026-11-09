using SchoolAdmission.BLL.Enums;

namespace SchoolAdmission.BLL.Models;

public class HoldCreationResult
{
    public HoldCreationStatus Status { get; set; }

    public HoldData? Hold { get; set; }
}