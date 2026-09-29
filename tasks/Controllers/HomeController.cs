using Microsoft.AspNetCore.Mvc;

namespace tasks.Controllers;

// Stage 0 + Stage 1: the front door that says what this API is.

[ApiController]
[Route("/")]
public class HomeController : ControllerBase
{
    /// <summary>
    /// Describes this API
    /// </summary>
    /// <returns>API name, version and the main endpoint</returns>
    [HttpGet]
    public IActionResult Root()
    {
        return Ok(new
        {
            name = "Task API",
            version = "1.0",
            endpoints = new[] { "/tasks" }
        });
    }

    /// <summary>
    /// Health check
    /// </summary>
    /// <returns>ok when the server is alive</returns>
    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { status = "ok" });
    }
}

