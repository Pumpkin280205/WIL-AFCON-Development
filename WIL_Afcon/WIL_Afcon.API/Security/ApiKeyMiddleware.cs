using System.Security.Cryptography;
using System.Text;

namespace WIL_Afcon.API.Security
{
    // Requires every API request to carry the shared key in the X-Api-Key
    // header. The MVC web app is the only intended caller, so the key lives
    // in both apps' configuration (user-secrets locally, environment
    // variables when hosted) and is never sent to the browser.
    //
    // Exempt paths:
    //  - the Hikvision webhook, which the terminal calls with its own token
    //    (checked in HikvisionEventController);
    //  - the SignalR hub, which the browser connects to directly and which
    //    only pushes notifications out;
    //  - Swagger in Development.
    public class ApiKeyMiddleware
    {
        public const string HeaderName = "X-Api-Key";

        private static readonly string[] ExemptPrefixes =
        {
            "/api/attendance/hikvision-event",
            "/attendanceHub",
            "/health",
        };

        private readonly RequestDelegate _next;
        private readonly byte[]? _expectedKey;
        private readonly bool _isDevelopment;
        private readonly ILogger<ApiKeyMiddleware> _logger;

        public ApiKeyMiddleware(
            RequestDelegate next,
            IConfiguration configuration,
            IWebHostEnvironment environment,
            ILogger<ApiKeyMiddleware> logger)
        {
            _next = next;
            _isDevelopment = environment.IsDevelopment();
            _logger = logger;

            var key = configuration["ApiSecurity:ApiKey"];
            _expectedKey = string.IsNullOrWhiteSpace(key)
                ? null
                : Encoding.UTF8.GetBytes(key);

            if (_expectedKey == null)
            {
                _logger.LogWarning(
                    _isDevelopment
                        ? "ApiSecurity:ApiKey is not configured - API key checks are disabled (Development only)."
                        : "ApiSecurity:ApiKey is not configured - all protected API requests will be rejected.");
            }
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (IsExempt(context.Request.Path) || IsAuthorised(context))
            {
                await _next(context);
                return;
            }

            _logger.LogWarning(
                "Rejected API request to {Path} from {RemoteIp}: missing or invalid API key.",
                context.Request.Path,
                context.Connection.RemoteIpAddress);

            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { error = "A valid API key is required." });
        }

        private bool IsExempt(PathString path)
        {
            if (_isDevelopment && path.StartsWithSegments("/swagger"))
            {
                return true;
            }

            return ExemptPrefixes.Any(prefix =>
                path.StartsWithSegments(prefix, StringComparison.OrdinalIgnoreCase));
        }

        private bool IsAuthorised(HttpContext context)
        {
            if (_expectedKey == null)
            {
                // Fail open only on a developer machine; fail closed everywhere else.
                return _isDevelopment;
            }

            var provided = context.Request.Headers[HeaderName].FirstOrDefault();

            if (string.IsNullOrEmpty(provided))
            {
                return false;
            }

            // Constant-time comparison so the key cannot be guessed byte by byte
            // from response timings.
            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(provided),
                _expectedKey);
        }
    }
}
