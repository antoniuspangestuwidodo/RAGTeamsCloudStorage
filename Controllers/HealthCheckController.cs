using Microsoft.AspNetCore.Mvc;

[Route("/")]
public class HealthCheckController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok("✅ Bot is running");
}