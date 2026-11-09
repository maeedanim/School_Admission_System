namespace SchoolAdmission.BLL.DTOs.Responses;

public class HoldResponse
{
    public Guid HoldId { get; set; }

    public Guid ClassId { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }
}