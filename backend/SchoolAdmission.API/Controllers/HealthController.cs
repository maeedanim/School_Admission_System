
using Microsoft.AspNetCore.Mvc;

namespace SchoolAdmission.API.Controllers;

[ApiController]
[Route("health")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult GetHealth()
    {
        return Ok(new
        {
            status = "Healthy",
            service = "SchoolAdmission.API"
        });
    }
}
