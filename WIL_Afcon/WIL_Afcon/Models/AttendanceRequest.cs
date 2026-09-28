using WIL_Afcon.Models.enums;

namespace WIL_Afcon.Models
{
    public class AttendanceRequest
    {
        public string Id { get; set; } = string.Empty;

        public string EmployeeId { get; set; } = string.Empty;

        public string? EmployeeName { get; set; }

        public RequestType RequestType { get; set; }

        public DateTime RequestDate { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string? Description { get; set; }

        public AttendanceRequestStatus Status { get; set; }

        public string? AdminComment { get; set; }

        public string? DocumentId { get; set; }

        public string? DocumentName { get; set; }

        public string? DocumentContentType { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? ReviewedAt { get; set; }

        public string? ReviewedBy { get; set; }
    }
}