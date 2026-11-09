namespace SchoolAdmission.BLL.Exceptions;

public class AdmissionException : Exception
{
    public string Code { get; }

    public AdmissionException(string code, string message)
        : base(message)
    {
        Code = code;
    }
}