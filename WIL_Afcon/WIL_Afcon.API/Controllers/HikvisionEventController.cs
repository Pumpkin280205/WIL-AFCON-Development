using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using WIL_Afcon.API.Configuration;
using WIL_Afcon.API.Models.Devices;
using WIL_Afcon.API.Services;
using WIL_Afcon.API.Services.Interfaces;

namespace WIL_Afcon.API.Controllers
{
    [ApiController]
    [Route("api/attendance")]
    public class HikvisionEventController : ControllerBase
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
        };

        private readonly IDeviceEventService _deviceEvents;
        private readonly HikvisionDeviceOptions _options;
        private readonly ILogger<HikvisionEventController> _logger;
        private readonly IHikvisionService _hikvisionService;
        private readonly IWebHostEnvironment _environment;

        public HikvisionEventController(
            IDeviceEventService deviceEvents,
            IOptions<HikvisionDeviceOptions> options,
            ILogger<HikvisionEventController> logger,
            IHikvisionService hikvisionService,
            IWebHostEnvironment environment)
        {
            _deviceEvents = deviceEvents;
            _options = options.Value;
            _logger = logger;
            _hikvisionService = hikvisionService;
            _environment = environment;
        }

        // POST: api/attendance/hikvision-event
        // Point the terminal's Network > Advanced > HTTP Listening setting here.
        // No [Consumes] filter: the terminal's content-type varies by firmware and
        // the body is read manually below.
        [HttpPost("hikvision-event")]
        [EnableRateLimiting("device")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<IActionResult> HikvisionEvent()
        {
            if (!IsRequestAuthorised())
            {
                _logger.LogWarning(
                    "Rejected Hikvision event from {RemoteIp}: missing or wrong token.",
                    HttpContext.Connection.RemoteIpAddress);
                return Unauthorized();
            }

            var payload = await ReadPayloadAsync();

            if (string.IsNullOrWhiteSpace(payload))
            {
                // Empty POST = the terminal's keep-alive. Record it as "device alive".
                await _deviceEvents.HandleHikvisionEventAsync(null);
                return Ok();
            }

            _logger.LogDebug("Hikvision payload received: {Payload}", payload);

            if (payload.TrimStart().StartsWith('<'))
            {
                _logger.LogWarning(
                    "Hikvision sent XML. Set the terminal's HTTP Listening 'Format' to JSON.");
                return Ok();
            }

            AccessControllerEvent? deviceEvent;
            try
            {
                deviceEvent = JsonSerializer
                    .Deserialize<HikvisionEventRoot>(payload, JsonOptions)?
                    .AccessControllerEvent;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Could not parse Hikvision JSON payload.");
                return Ok(); // 200 so the terminal stops retrying a body we cannot use
            }

            var outcome = await _deviceEvents.HandleHikvisionEventAsync(deviceEvent);
            _logger.LogInformation("Hikvision event outcome: {Outcome}", outcome);

            return Ok();
        }

        // GET: api/attendance/device/health
        [HttpGet("device/health")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public ActionResult<DeviceHealthResponse> Health()
            => Ok(_deviceEvents.GetHealth());

       
        private bool IsRequestAuthorised()
        {
            var expected = _options.InboundToken;

            var provided = Request.Query["token"].FirstOrDefault()
                           ?? Request.Headers["X-Device-Token"].FirstOrDefault();

            if (string.IsNullOrWhiteSpace(expected))
            {
                // No token configured: only tolerate this on a developer machine,
                // otherwise anyone could post fake clock-ins.
                if (_environment.IsDevelopment())
                {
                    return true;
                }

                _logger.LogError(
                    "HikvisionDevice:InboundToken is not configured; rejecting device events.");
                return false;
            }

            if (string.IsNullOrEmpty(provided))
            {
                return false;
            }

            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(provided),
                Encoding.UTF8.GetBytes(expected));
        }


        // DELETE: api/attendance/device/employee/{deviceUserId}
        [HttpDelete("device/employee/{deviceUserId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status502BadGateway)]
        public async Task<IActionResult> DeleteEmployeeFromDevice(string deviceUserId)
        {
            if (string.IsNullOrWhiteSpace(deviceUserId))
            {
                return Problem(
                    detail: "Device user ID is required.",
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Invalid request");
            }

            var deleted = await _hikvisionService.DeleteEmployeeAsync(deviceUserId);

            if (!deleted)
            {
                return Problem(
                    detail: "The employee could not be removed from the Hikvision device.",
                    statusCode: StatusCodes.Status502BadGateway,
                    title: "Device unavailable");
            }

            return Ok(new
            {
                message = "Employee removed from the Hikvision device.",
                deviceUserId
            });
        }


        private async Task<string?> ReadPayloadAsync()
        {
            if (Request.HasFormContentType)
            {
                // The terminal sends multipart/form-data: one text/JSON part plus
                // optional face-image parts. The JSON part may arrive as a form
                // field or as a file, depending on firmware - check both.
                var form = await Request.ReadFormAsync();

                foreach (var field in form)
                {
                    var value = field.Value.ToString();
                    if (LooksLikePayload(value))
                    {
                        return value;
                    }
                }

                foreach (var file in form.Files)
                {
                    using var reader = new StreamReader(file.OpenReadStream());
                    var text = await reader.ReadToEndAsync();
                    if (LooksLikePayload(text))
                    {
                        return text;
                    }
                }

                return null;
            }

            using var bodyReader = new StreamReader(Request.Body);
            return await bodyReader.ReadToEndAsync();
        }





        private static bool LooksLikePayload(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            var trimmed = text.TrimStart();
            return trimmed.StartsWith('{') || trimmed.StartsWith('<');
        }
    }
}
