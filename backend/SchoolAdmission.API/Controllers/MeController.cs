
using Microsoft.AspNetCore.Mvc;
using SchoolAdmission.BLL.Interfaces;

namespace SchoolAdmission.API.Controllers;

[ApiController]
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
        [FromHeader(Name = "X-Student-Id")] string? studentId,
        CancellationToken cancellationToken)
    {
        var response = await _admissionService.GetStudentHoldsAsync(
            studentId ?? string.Empty,
            cancellationToken);

        return Ok(response);
    }
}
