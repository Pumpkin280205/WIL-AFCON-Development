using WIL_Afcon.Models;

namespace WIL_Afcon.Models.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalEmployees { get; set; }

        public int PresentToday { get; set; }

        public int OnLeaveToday { get; set; }

        public int AbsentToday { get; set; }

        public List<AdminTodayAttendanceViewModel> TodayAttendance { get; set; }
            = new();

        public List<MonthlyAttendanceViewModel> MonthlyAttendance { get; set; }
            = new();
    }


    public class AdminTodayAttendanceViewModel
    {
        public string EmployeeNumber { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;
    }


    public class MonthlyAttendanceViewModel
    {
        public string Month { get; set; } = string.Empty;

        public int Present { get; set; }

        public int Absent { get; set; }

        public int Late { get; set; }
    }
}