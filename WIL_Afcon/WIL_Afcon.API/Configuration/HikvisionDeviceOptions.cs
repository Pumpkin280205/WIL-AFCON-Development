namespace WIL_Afcon.API.Configuration
{
    /// <summary>
    /// Settings for the Hikvision DS-K1T67MF access terminal.
    /// Bound from the "HikvisionDevice" section of appsettings.
    /// Keep the real host and password out of the committed appsettings.json
    /// (use appsettings.Development.json or `dotnet user-secrets`).
    /// </summary>
    public class HikvisionDeviceOptions
    {
        public const string SectionName = "HikvisionDevice";

        /// <summary>LAN IP / hostname of the terminal. Only needed for the health check
        /// and any future server-to-device calls (enrolment, polling fallback).</summary>
        public string Host { get; set; } = string.Empty;

        public int Port { get; set; } = 80;

        public string Username { get; set; } = "admin";

        public string Password { get; set; } = string.Empty;

        public bool UseHttps { get; set; } = false;

        /// <summary>Punches for the same person repeated inside this window are ignored.
        /// The terminal double-fires and re-POSTs until it gets HTTP 200.</summary>
        public int DedupeWindowSeconds { get; set; } = 60;

        /// <summary>Optional shared secret. When set, the terminal must send it as
        /// `?token=...` on the HTTP Listening URL or an `X-Device-Token` header.</summary>
        public string InboundToken { get; set; } = string.Empty;
    }
}
