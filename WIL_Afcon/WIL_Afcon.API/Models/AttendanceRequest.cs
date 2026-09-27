using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using WIL_Afcon.API.Models.enums;

namespace WIL_Afcon.API.Models
{
    public class AttendanceRequest
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonRepresentation(BsonType.ObjectId)]
        public string EmployeeId { get; set; } = string.Empty;

        public RequestType RequestType { get; set; }

        public DateTime RequestDate { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string? Description { get; set; }

        public AttendanceRequestStatus Status { get; set; }
            = AttendanceRequestStatus.Pending;

        public string? AdminComment { get; set; }

        public string? DocumentId { get; set; }

        public string? DocumentName { get; set; }

        public string? DocumentContentType { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ReviewedAt { get; set; }

        public string? ReviewedBy { get; set; }
    }
}