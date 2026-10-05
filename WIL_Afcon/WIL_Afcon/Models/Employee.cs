using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WIL_Afcon.Models
{
    public class Employee
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "Employee number is required.")]
        [StringLength(20)]
        public string EmployeeNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "First name is required.")]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Enter a valid phone number.")]
        [StringLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [BsonRepresentation(BsonType.ObjectId)]
        [Required(ErrorMessage = "Select a department.")]
        public string DepartmentId { get; set; } = string.Empty;

        public string? UserId { get; set; }

        // The terminal's "Employee No.": letters, digits, '-' and '_' only.
        [StringLength(32)]
        [RegularExpression(@"^[A-Za-z0-9_-]+$", ErrorMessage = "Device User ID may only contain letters, numbers, '-' and '_'.")]
        public string? DeviceUserId { get; set; }

       // public string? FingerprintReference { get; set; }

        public bool IsActive { get; set; } = true;
    }
}