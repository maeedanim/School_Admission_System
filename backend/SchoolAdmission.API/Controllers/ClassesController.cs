
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using SchoolAdmission.BLL.DTOs.Requests;
using SchoolAdmission.BLL.Interfaces;

namespace SchoolAdmission.API.Controllers;

[ApiController]
[Route("classes")]
[Authorize]
public class ClassesController : ControllerBase
{
    private const string AdminRole = "Admin";

    private readonly IAdmissionService _admissionService;

    public ClassesController(IAdmissionService admissionService)
    {
        _admissionService = admissionService;
    }

    // POST /classes
    // Only administrators can create classes.
    [HttpPost]
    [Authorize(Roles = AdminRole)]
    public async Task<IActionResult> CreateClass(
        [FromBody] CreateClassRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _admissionService.CreateClassAsync(
            request,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetClassById),
            new { classId = response.Id },
            response);
    }

    // GET /classes
    // Authenticated students and administrators can view classes.
    [HttpGet]
    public async Task<IActionResult> GetClasses(
        CancellationToken cancellationToken)
    {
        var response = await _admissionService.GetClassesAsync(
            cancellationToken);

        return Ok(response);
    }

    // GET /classes/{classId}
    // Authenticated students and administrators can view class details.
    [HttpGet("{classId:guid}")]
    public async Task<IActionResult> GetClassById(
        Guid classId,
        CancellationToken cancellationToken)
    {
        var response = await _admissionService.GetClassByIdAsync(
            classId,
            cancellationToken);

        return Ok(response);
    }
}
