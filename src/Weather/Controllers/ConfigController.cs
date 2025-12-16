using Microsoft.AspNetCore.Mvc;
using Weather.Configuration;
using Weather.Models.Requests;
using Weather.Models.Responses;

namespace Weather.Controllers;

/// <summary>
/// Controller for managing test configuration at runtime.
/// Allows dynamic control of health check behavior for testing purposes.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ConfigController : ControllerBase
{
    private readonly ITestConfigurationState _testState;
    private readonly ILogger<ConfigController> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="ConfigController"/>.
    /// </summary>
    /// <param name="testState">The test configuration state service.</param>
    /// <param name="logger">The logger.</param>
    public ConfigController(
        ITestConfigurationState testState,
        ILogger<ConfigController> logger)
    {
        _testState = testState ?? throw new ArgumentNullException(nameof(testState));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets the current test configuration state.
    /// </summary>
    /// <returns>The current test configuration.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(TestConfigurationResponse), StatusCodes.Status200OK)]
    public ActionResult<TestConfigurationResponse> GetConfiguration()
    {
        var response = MapToResponse();
        return Ok(response);
    }

    /// <summary>
    /// Updates the test configuration state.
    /// Only specified properties in the request will be updated.
    /// </summary>
    /// <param name="request">The configuration update request.</param>
    /// <returns>The updated test configuration.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(TestConfigurationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<TestConfigurationResponse> UpdateConfiguration(
        [FromBody] TestConfigurationRequest request)
    {
        if (request == null)
        {
            return BadRequest("Request body is required");
        }

        // Apply only the properties that were specified in the request
        if (request.ForceStartupFail.HasValue)
        {
            _testState.ForceStartupFail = request.ForceStartupFail.Value;
            _logger.LogInformation("ForceStartupFail set to {Value}", request.ForceStartupFail.Value);
        }

        if (request.ForceReadyFail.HasValue)
        {
            _testState.ForceReadyFail = request.ForceReadyFail.Value;
            _logger.LogInformation("ForceReadyFail set to {Value}", request.ForceReadyFail.Value);
        }

        if (request.ForceLiveFail.HasValue)
        {
            _testState.ForceLiveFail = request.ForceLiveFail.Value;
            _logger.LogInformation("ForceLiveFail set to {Value}", request.ForceLiveFail.Value);
        }

        if (request.StartupDelayMs.HasValue)
        {
            _testState.StartupDelayMs = request.StartupDelayMs.Value;
            _logger.LogInformation("StartupDelayMs set to {Value}ms", request.StartupDelayMs.Value);
        }

        if (request.ReadyDelayMs.HasValue)
        {
            _testState.ReadyDelayMs = request.ReadyDelayMs.Value;
            _logger.LogInformation("ReadyDelayMs set to {Value}ms", request.ReadyDelayMs.Value);
        }

        if (request.LiveDelayMs.HasValue)
        {
            _testState.LiveDelayMs = request.LiveDelayMs.Value;
            _logger.LogInformation("LiveDelayMs set to {Value}ms", request.LiveDelayMs.Value);
        }

        var response = MapToResponse();
        return Ok(response);
    }

    private TestConfigurationResponse MapToResponse()
    {
        return new TestConfigurationResponse
        {
            ForceStartupFail = _testState.ForceStartupFail,
            ForceReadyFail = _testState.ForceReadyFail,
            ForceLiveFail = _testState.ForceLiveFail,
            StartupDelayMs = _testState.StartupDelayMs,
            ReadyDelayMs = _testState.ReadyDelayMs,
            LiveDelayMs = _testState.LiveDelayMs
        };
    }
}
