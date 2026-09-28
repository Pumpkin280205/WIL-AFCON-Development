namespace WIL_Afcon.API.Configuration
{
    /// <summary>
    /// Working-day rules used to decide what each biometric scan means.
    /// Bound from the "WorkSchedule" section of appsettings.json.
    /// </summary>
    public class WorkScheduleOptions
    {
        public const string SectionName = "WorkSchedule";

        /// <summary>Earliest time of day a scan is accepted (and earliest clock-in).</summary>
        public TimeSpan EarliestClockIn { get; set; } = new(6, 0, 0);

        /// <summary>Latest time of day a first scan still counts as a clock-in.</summary>
        public TimeSpan LatestClockIn { get; set; } = new(12, 0, 0);

        /// <summary>Clock-ins after this time are marked Late.</summary>
        public TimeSpan LateAfter { get; set; } = new(9, 0, 0);

        /// <summary>From this time of day a scan counts as clock-out (end of work day).</summary>
        public TimeSpan ClockOutFrom { get; set; } = new(15, 0, 0);

        /// <summary>Scans after this time of day are rejected.</summary>
        public TimeSpan LatestScan { get; set; } = new(21, 0, 0);

        /// <summary>Scans closer together than this are rejected (stops accidental double scans).</summary>
        public int MinMinutesBetweenScans { get; set; } = 5;

        /// <summary>How many breaks an employee may take per day.</summary>
        public int MaxBreaksPerDay { get; set; } = 1;
    }
}