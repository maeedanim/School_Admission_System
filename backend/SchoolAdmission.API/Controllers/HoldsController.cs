
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolAdmission.BLL.Interfaces;

namespace SchoolAdmission.API.Controllers;

[ApiController]
[Authorize(Roles = "Student")]
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
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out Guid userId))
        {
            return Unauthorized();
        }

        var response = await _admissionService.CreateHoldAsync(
            classId,
            userId,
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
        if (!TryGetCurrentUserId(out Guid userId))
        {
            return Unauthorized();
        }

        var response = await _admissionService.GetHoldByIdAsync(
            holdId,
            userId,
            cancellationToken);

        return Ok(response);
    }

    // POST /holds/{holdId}/confirm
    [HttpPost("/holds/{holdId:guid}/confirm")]
    public async Task<IActionResult> ConfirmHold(
        Guid holdId,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out Guid userId))
        {
            return Unauthorized();
        }

        var response = await _admissionService.ConfirmHoldAsync(
            holdId,
            userId,
            cancellationToken);

        return Ok(response);
    }

    // DELETE /holds/{holdId}
    [HttpDelete("/holds/{holdId:guid}")]
    public async Task<IActionResult> CancelHold(
        Guid holdId,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out Guid userId))
        {
            return Unauthorized();
        }

        await _admissionService.CancelHoldAsync(
            holdId,
            userId,
            cancellationToken);

        return NoContent();
    }

    private bool TryGetCurrentUserId(out Guid userId)
    {
        string? userIdClaim = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        return Guid.TryParse(userIdClaim, out userId);
    }
}
