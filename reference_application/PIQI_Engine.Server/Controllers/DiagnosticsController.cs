using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace PIQI_Engine.Server.Controllers;

/// <summary>
/// Provides diagnostic endpoints for checking the health and version of the API.
/// </summary>
[Route("[controller]")]
[ApiController]
public class DiagnosticsController : ControllerBase
{
    /// <summary>
    /// Checks the health status of the API.
    /// </summary>
    /// <returns>
    /// An <see cref="ActionResult{string}"/> containing "OK" if the API is running and reachable.
    /// </returns>
    [Route("status")]
    [HttpGet]
    public ActionResult<string> CheckHealth() => Ok("OK");

    /// <summary>
    /// Retrieves the current version of the running assembly.
    /// </summary>
    /// <returns>
    /// An <see cref="ActionResult{string}"/> containing the version string of the executing assembly.
    /// Returns <c>null</c> if the version information is unavailable.
    /// </returns>
    [Route("version")]
    [HttpGet]
    public ActionResult<string> GetVersionInfo() =>
        Ok(Assembly.GetExecutingAssembly().GetName()?.Version?.ToString());
}
