using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WIL_Afcon.API.Configuration;
using WIL_Afcon.API.Models.Devices;
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

        public HikvisionEventController(
            IDeviceEventService deviceEvents,
            IOptions<HikvisionDeviceOptions> options,
            ILogger<HikvisionEventController> logger)
        {
            _deviceEvents = deviceEvents;
            _options = options.Value;
            _logger = logger;
        }

        // POST: api/attendance/hikvision-event
        // Point the terminal's Network > Advanced > HTTP Listening setting here.
        // No [Consumes] filter: the terminal's content-type varies by firmware and
        // the body is read manually below.
        [HttpPost("hikvision-event")]
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

            _logger.LogInformation("Hikvision payload received: {Payload}", payload);

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

            // Always 200 quickly - a non-2xx makes the terminal queue and re-POST.
            return Ok();
        }

        // GET: api/attendance/device/health
        [HttpGet("device/health")]
        public ActionResult<DeviceHealthResponse> Health()
            => Ok(_deviceEvents.GetHealth());

        private bool IsRequestAuthorised()
        {
            var expected = _options.InboundToken;
            if (string.IsNullOrWhiteSpace(expected))
            {
                return true; // no token configured - accept everything (LAN-only setup)
            }

            var provided = Request.Query["token"].FirstOrDefault()
                           ?? Request.Headers["X-Device-Token"].FirstOrDefault();

            return string.Equals(provided, expected, StringComparison.Ordinal);
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
