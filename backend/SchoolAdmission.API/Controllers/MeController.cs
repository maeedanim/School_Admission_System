
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolAdmission.BLL.Interfaces;

namespace SchoolAdmission.API.Controllers;

[ApiController]
[Authorize(Roles = "Student")]
public class MeController : ControllerBase
{
    private readonly IAdmissionService _admissionService;

    public MeController(IAdmissionService admissionService)
    {
        _admissionService = admissionService;
    }

    // GET /me/holds
    [HttpGet("/me/holds")]
    public async Task<IActionResult> GetMyHolds(
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out Guid userId))
        {
            return Unauthorized();
        }

        var response = await _admissionService.GetStudentHoldsAsync(
            userId,
            cancellationToken);

        return Ok(response);
    }

    private bool TryGetCurrentUserId(out Guid userId)
    {
        string? userIdClaim = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        return Guid.TryParse(userIdClaim, out userId);
    }
}
