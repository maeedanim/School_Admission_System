
using System.ComponentModel.DataAnnotations;

namespace SchoolAdmission.API.Models.Auth;

public class StudentRegisterRequest
{
    [Required]
    [StringLength(200, MinimumLength = 2)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [Compare(nameof(Password))]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required]
    [RegularExpression(
        @"^[0-9]{17}$",
        ErrorMessage = "Birth Registration Number must contain exactly 17 digits.")]
    public string BirthRegistrationNumber { get; set; } = string.Empty;
}
