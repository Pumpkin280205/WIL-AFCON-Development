using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WIL_Afcon.Models
{
    public class Employee
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        public string EmployeeNumber { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        [BsonRepresentation(BsonType.ObjectId)]
        public string DepartmentId { get; set; } = string.Empty;

        public string? UserId { get; set; }

        public string? DeviceUserId { get; set; }

       // public string? FingerprintReference { get; set; }

        public bool IsActive { get; set; } = true;
    }
}