namespace SchoolAdmission.BLL.DTOs.Responses;

public class CreateClassResponse
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int TotalSeats { get; set; }
}