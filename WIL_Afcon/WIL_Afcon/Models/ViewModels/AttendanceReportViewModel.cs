using System.Globalization;

namespace WIL_Afcon.Models.ViewModels
{
    /// <summary>
    /// Monthly attendance register for ONE employee
    /// (replaces the manual Attendance_Register.xlsx sheet).
    /// </summary>
    public class AttendanceReportViewModel
    {
        public string EmployeeId { get; set; } = string.Empty;

        public string EmployeeName { get; set; } = string.Empty;

        public string EmployeeNumber { get; set; } = string.Empty;

        public string DepartmentName { get; set; } = string.Empty;

        public int Year { get; set; }

        public int Month { get; set; }

        public string MonthName =>
            CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(Month);

        public double StandardDailyHours { get; set; }

        public DateTime GeneratedOn { get; set; }

        public List<AttendanceReportDayViewModel> Days { get; set; } = new();

        // ----- Monthly totals -----
        public double TotalHoursWorked { get; set; }

        public double TotalOvertimeHours { get; set; }

        public double TotalSundayHours { get; set; }

        public double TotalPublicHolidayHours { get; set; }

        public int DaysPresent { get; set; }

        public int DaysLate { get; set; }

        public int DaysAbsent { get; set; }

        public int DaysOnLeave { get; set; }
    }

    /// <summary>
    /// One row (one calendar day) of the monthly attendance register.
    /// </summary>
    public class AttendanceReportDayViewModel
    {
        public DateTime Date { get; set; }

        public int DayNumber => Date.Day;

        public string DayOfWeekName => Date.DayOfWeek.ToString();

        public bool IsSaturday => Date.DayOfWeek == DayOfWeek.Saturday;

        public bool IsSunday => Date.DayOfWeek == DayOfWeek.Sunday;

        public bool IsPublicHoliday { get; set; }

        public string? PublicHolidayName { get; set; }

        public bool HasRecord { get; set; }

        // Times are already converted to local time.
        public DateTime? StartingTime { get; set; }

        public DateTime? MealOff { get; set; }

        public DateTime? MealOn { get; set; }

        public int BreakCount { get; set; }

        public int BreakMinutes { get; set; }

        public DateTime? FinishingTime { get; set; }

        public double HoursWorked { get; set; }

        /// <summary>
        /// Only filled on the last day of each week (Sunday) and on the
        /// last day of the month, so it reads like the "Each Week" column.
        /// </summary>
        public double? WeekTotalHours { get; set; }

        public double OvertimeHours { get; set; }

        public double SundayHours { get; set; }

        public double PublicHolidayHours { get; set; }

        public string Remarks { get; set; } = string.Empty;
    }
}