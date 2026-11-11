
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolAdmission.BLL.Interfaces;

namespace SchoolAdmission.API.Controllers;

[ApiController]
[Route("admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAdmissionService _admissionService;

    public AdminController(IAdmissionService admissionService)
    {
        _admissionService = admissionService;
    }

    [HttpGet("applications")]
    public async Task<IActionResult> GetAllApplications(
        CancellationToken cancellationToken)
    {
        var applications = await _admissionService.GetAllApplicationsAsync(
            cancellationToken);

        return Ok(applications);
    }
}
