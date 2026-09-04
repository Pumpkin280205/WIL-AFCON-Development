using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using WIL_Afcon.API.Models.Enums;

namespace WIL_Afcon.API.Models
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

        [BsonRepresentation(BsonType.String)]
        public AttendanceStatus AttendanceStatus { get; set; }
            = AttendanceStatus.Absent;

        public int WorkingMinutes { get; set; }

        public List<BreakRecord> Breaks { get; set; } = new();

        public string? Documentation { get; set; }

        [BsonIgnore]
        public bool IsOnBreak => Breaks.Any(b => b.EndTime == null);
    }
}