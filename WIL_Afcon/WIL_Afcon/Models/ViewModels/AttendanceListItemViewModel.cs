using WIL_Afcon.Models.Enums;

namespace WIL_Afcon.Models.ViewModels
{
    public class AttendanceListItemViewModel
    {
        public string Id { get; set; } = string.Empty;

        public string EmployeeName { get; set; } = string.Empty;

        public string EmployeeNumber { get; set; } = string.Empty;

        public string DepartmentName { get; set; } = string.Empty;

        public DateTime AttendanceDate { get; set; }

        public DateTime? ClockInTime { get; set; }

        public DateTime? ClockOutTime { get; set; }

        public AttendanceStatus AttendanceStatus { get; set; }

        public int WorkingMinutes { get; set; }
    }
}
