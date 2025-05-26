using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("/")]
public class HealthCheckController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok("✅ Bot is healthy");
}