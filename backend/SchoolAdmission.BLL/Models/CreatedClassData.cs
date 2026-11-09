namespace SchoolAdmission.BLL.Models;

public class CreatedClassData
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int TotalSeats { get; set; }
}