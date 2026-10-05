using System.ComponentModel.DataAnnotations;
using WIL_Afcon.Models.enums;

namespace WIL_Afcon.Models
{
    public class AttendanceRequest : IValidatableObject
    {
        public string Id { get; set; } = string.Empty;

        public string EmployeeId { get; set; } = string.Empty;

        public string? EmployeeName { get; set; }

        public RequestType RequestType { get; set; }

        public DateTime RequestDate { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        public AttendanceRequestStatus Status { get; set; }

        public string? AdminComment { get; set; }

        public string? DocumentId { get; set; }

        public string? DocumentName { get; set; }

        public string? DocumentContentType { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? ReviewedAt { get; set; }

        public string? ReviewedBy { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (StartDate.HasValue && EndDate.HasValue && EndDate < StartDate)
            {
                yield return new ValidationResult(
                    "End date cannot be before the start date.",
                    new[] { nameof(EndDate) });
            }
        }
    }
}