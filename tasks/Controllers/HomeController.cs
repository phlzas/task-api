using Microsoft.AspNetCore.Mvc;

namespace tasks.Controllers;

// Stage 0 + Stage 1 live here: the front door that says what this API is.

[ApiController]
[Route("/")]
public class HomeController : ControllerBase
{
    // =============================================================
    // STAGE 0 — DONE. This works right now. Verify it:
    //   curl -i http://localhost:5241/
    // Expected: 200 + JSON
    // =============================================================
    [HttpGet]
    public IActionResult Root()
    {
        return Ok(new { message = "Hello, server" });
    }

    // =============================================================
    // STAGE 1 — your first real endpoint
    //
    // TODO 2: GET /health -> { "status": "ok" }
    //
    // (The root endpoint above returns only a message. The assignment
    //  also wants it to describe the API. Either extend the object
    //  above or add a second action — your choice, but pick one.)
    //
    // Checkpoint: both URLs return JSON in the browser and via curl
    // Commit:     Stage 1: root and health endpoints
    // =============================================================
}
