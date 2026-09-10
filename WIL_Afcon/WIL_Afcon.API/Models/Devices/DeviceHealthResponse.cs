namespace WIL_Afcon.API.Models.Devices
{
    /// <summary>
    /// Snapshot of what the biometric terminal has sent this API since it started.
    /// Served by GET /api/attendance/device/health so a dead terminal is visible.
    /// In-memory only - resets when the API restarts.
    /// </summary>
    public class DeviceHealthResponse
    {
        public string? ExpectedHost { get; set; }

        public DateTime? LastEventUtc { get; set; }

        public DateTime? LastPunchAppliedUtc { get; set; }

        public int? SecondsSinceLastEvent { get; set; }

        public long TotalEventsReceived { get; set; }

        public long TotalPunchesApplied { get; set; }
    }
}
