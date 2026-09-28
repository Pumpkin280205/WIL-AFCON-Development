using WIL_Afcon.Models;

namespace WIL_Afcon.Models.ViewModels
{
    public class EmployeeDashboardViewModel
    {
        public Employee Employee { get; set; } = new();

        public Attendance? TodayAttendance { get; set; }

        public List<Attendance> RecentAttendance { get; set; } = new();

        public int DaysPresent { get; set; }

        public int DaysAbsent { get; set; }

        public int LateArrivals { get; set; }

        public int TotalMinutesThisMonth { get; set; }

        public Dictionary<string, int> WeeklyMinutes { get; set; } = new();
    }
}