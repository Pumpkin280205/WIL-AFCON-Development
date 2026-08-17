using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WIL_Afcon.Models
{
    public class Attendance
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonRepresentation(BsonType.ObjectId)]
        public string EmployeeId { get; set; } = string.Empty;

        public DateTime AttendanceDate { get; set; }

        public DateTime? ClockInTime { get; set; }

        public DateTime? ClockOutTime { get; set; }

        public string AttendanceStatus { get; set; } = string.Empty;

        public int WorkingMinutes { get; set; }

        public List<BreakRecord> Breaks { get; set; } = new();

        public string? Documentation { get; set; }
    }
}