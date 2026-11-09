namespace SchoolAdmission.BLL.DTOs.Responses;

public class ConfirmHoldResponse
{
    public Guid HoldId { get; set; }

    public string Status { get; set; } = "CONFIRMED";
}