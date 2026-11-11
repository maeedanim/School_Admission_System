
namespace SchoolAdmission.BLL.DTOs.Responses;

public class AdminApplicationResponse
{
    public Guid ApplicationId { get; set; }

    public string? StudentName { get; set; }

    public string? StudentEmail { get; set; }

    public string StudentId { get; set; } = string.Empty;

    public Guid ClassId { get; set; }

    public string ClassName { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime ExpiresAt { get; set; }

    public DateTime? ConfirmedAt { get; set; }

    public DateTime? CancelledAt { get; set; }
}
