using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Coka.Social.Listening.Core.DTOs;

namespace Coka.Social.Listening.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(ApiResponse<object>.Ok(new
        {
            Status = "Healthy",
            Timestamp = DateTime.Now,
            Service = "Coka.Social.Listening.API"
        }));
    }

    [HttpGet("secured")]
    public IActionResult Secured()
    {
        return Ok(ApiResponse<object>.Ok(new
        {
            Message = "You are authenticated!",
            User = User.Identity?.Name
        }));
    }
}
