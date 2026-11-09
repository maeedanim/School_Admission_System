
using Microsoft.AspNetCore.Mvc;
using SchoolAdmission.BLL.Interfaces;

namespace SchoolAdmission.API.Controllers;

[ApiController]
public class HoldsController : ControllerBase
{
    private readonly IAdmissionService _admissionService;

    public HoldsController(IAdmissionService admissionService)
    {
        _admissionService = admissionService;
    }

    // POST /classes/{classId}/holds
    [HttpPost("/classes/{classId:guid}/holds")]
    public async Task<IActionResult> CreateHold(
        Guid classId,
        [FromHeader(Name = "X-Student-Id")] string? studentId,
        CancellationToken cancellationToken)
    {
        var response = await _admissionService.CreateHoldAsync(
            classId,
            studentId ?? string.Empty,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetHoldById),
            new { holdId = response.HoldId },
            response);
    }

    // GET /holds/{holdId}
    [HttpGet("/holds/{holdId:guid}")]
    public async Task<IActionResult> GetHoldById(
        Guid holdId,
        CancellationToken cancellationToken)
    {
        var response = await _admissionService.GetHoldByIdAsync(
            holdId,
            cancellationToken);

        return Ok(response);
    }

    // POST /holds/{holdId}/confirm
    [HttpPost("/holds/{holdId:guid}/confirm")]
    public async Task<IActionResult> ConfirmHold(
        Guid holdId,
        [FromHeader(Name = "X-Student-Id")] string? studentId,
        CancellationToken cancellationToken)
    {
        var response = await _admissionService.ConfirmHoldAsync(
            holdId,
            studentId ?? string.Empty,
            cancellationToken);

        return Ok(response);
    }

    // DELETE /holds/{holdId}
    [HttpDelete("/holds/{holdId:guid}")]
    public async Task<IActionResult> CancelHold(
        Guid holdId,
        [FromHeader(Name = "X-Student-Id")] string? studentId,
        CancellationToken cancellationToken)
    {
        await _admissionService.CancelHoldAsync(
            holdId,
            studentId ?? string.Empty,
            cancellationToken);

        return NoContent();
    }
}
