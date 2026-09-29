using Microsoft.AspNetCore.Mvc;
using TaskApi.Models;

namespace TaskApi.Controllers;

/// <summary>Service metadata and liveness endpoints.</summary>
[ApiController]
[Route("/")]
[Produces("application/json")]
public sealed class ApiInfoController : ControllerBase
{
    /// <summary>Describes the API and its collection endpoint.</summary>
    /// <response code="200">API name, version and the main endpoint path.</response>
    [HttpGet("")]
    [ProducesResponseType(typeof(ApiInfo), StatusCodes.Status200OK)]
    public ActionResult<ApiInfo> GetApiInfo() =>
        Ok(new ApiInfo("Task API", "1.0", ["/tasks"]));

    /// <summary>Liveness probe.</summary>
    /// <response code="200">Always ok while the process is running.</response>
    [HttpGet("health")]
    [ProducesResponseType(typeof(HealthStatus), StatusCodes.Status200OK)]
    public ActionResult<HealthStatus> GetHealth() => Ok(new HealthStatus("ok"));
}

/// <summary>Payload returned by the root endpoint.</summary>
/// <param name="Name">Name of the API.</param>
/// <param name="Version">Version of the API.</param>
/// <param name="Endpoints">Collection endpoints exposed by the API.</param>
public sealed record ApiInfo(string Name, string Version, string[] Endpoints);

/// <summary>Payload returned by the health endpoint.</summary>
/// <param name="Status">Always <c>ok</c> when the service is up.</param>
public sealed record HealthStatus(string Status);
