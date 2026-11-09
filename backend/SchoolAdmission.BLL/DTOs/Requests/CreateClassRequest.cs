using System.ComponentModel.DataAnnotations;

namespace SchoolAdmission.BLL.DTOs.Requests;

public class CreateClassRequest
{
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int TotalSeats { get; set; }
}