using System.Text.Json.Serialization;

namespace WIL_Afcon.API.Models.Devices
{
    /// <summary>
    /// Shape of the JSON body a Hikvision terminal (DS-K1T67MF) POSTs to the
    /// Network &gt; Advanced &gt; HTTP Listening URL when a person is authenticated.
    /// Only the fields this API uses are mapped; the terminal sends many more.
    /// The terminal's HTTP Listening "Format" must be set to JSON.
    /// </summary>
    public class HikvisionEventRoot
    {
        [JsonPropertyName("eventType")]
        public string? EventType { get; set; }

        [JsonPropertyName("dateTime")]
        public DateTimeOffset? DateTime { get; set; }

        [JsonPropertyName("AccessControllerEvent")]
        public AccessControllerEvent? AccessControllerEvent { get; set; }
    }

    public class AccessControllerEvent
    {
        /// <summary>The person's "Employee No." as enrolled on the terminal.
        /// Maps to Employee.DeviceUserId in this system.</summary>
        [JsonPropertyName("employeeNoString")]
        public string? EmployeeNoString { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        /// <summary>checkIn / checkOut / breakIn / breakOut / checkInAndOut, or empty
        /// when the terminal's Attendance Status feature is disabled.</summary>
        [JsonPropertyName("attendanceStatus")]
        public string? AttendanceStatus { get; set; }

        [JsonPropertyName("majorEventType")]
        public int? MajorEventType { get; set; }

        [JsonPropertyName("subEventType")]
        public int? SubEventType { get; set; }

        [JsonPropertyName("currentVerifyMode")]
        public string? CurrentVerifyMode { get; set; }

        /// <summary>Per-event serial number from the terminal. Stable across re-POSTs of
        /// the same punch, so it is the best de-duplication key when present.</summary>
        [JsonPropertyName("serialNo")]
        public long? SerialNo { get; set; }

        [JsonPropertyName("doorNo")]
        public int? DoorNo { get; set; }
    }
}
