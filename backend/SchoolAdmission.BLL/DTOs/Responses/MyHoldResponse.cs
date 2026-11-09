namespace SchoolAdmission.BLL.DTOs.Responses;

public class MyHoldResponse
{
    public Guid HoldId { get; set; }

    public Guid ClassId { get; set; }

    public string ClassName { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }
}