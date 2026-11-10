namespace SchoolAdmission.BLL.Models.Auth;

public enum RegistrationStatus
{
    Succeeded,
    InvalidInput,
    DuplicateEmail,
    DuplicateBirthRegistrationNumber,
    Failed
}



public class RegistrationResult
{
    public RegistrationStatus Status { get; set; }

    public bool Succeeded => Status == RegistrationStatus.Succeeded;

    public Guid? UserId { get; set; }

    public IReadOnlyList<string> Errors { get; set; }
        = Array.Empty<string>();
}