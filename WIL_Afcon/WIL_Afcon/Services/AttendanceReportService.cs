using System.Globalization;
using System.Text;
using WIL_Afcon.Models;
using WIL_Afcon.Models.Enums;
using WIL_Afcon.Models.ViewModels;
using WIL_Afcon.Repositories.Interfaces;
using WIL_Afcon.Services.Interfaces;

namespace WIL_Afcon.Services
{
    /// <summary>
    /// Generates the monthly attendance register for an employee
    /// from the attendance records captured by the Hikvision device.
    /// </summary>
    public class AttendanceReportService : IAttendanceReportService
    {
        // Normal hours per day. Anything worked above this on a
        // Monday - Saturday (that is not a public holiday) is overtime.
        private const double StandardDailyHours = 8.0;

        private readonly IAttendanceRepository _attendanceRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IDepartmentRepository _departmentRepository;

        public AttendanceReportService(
            IAttendanceRepository attendanceRepository,
            IEmployeeRepository employeeRepository,
            IDepartmentRepository departmentRepository)
        {
            _attendanceRepository = attendanceRepository;
            _employeeRepository = employeeRepository;
            _departmentRepository = departmentRepository;
        }

        public async Task<AttendanceReportViewModel?> GenerateMonthlyReportAsync(
            string employeeId,
            int year,
            int month)
        {
            if (string.IsNullOrWhiteSpace(employeeId))
            {
                return null;
            }

            var employee = await _employeeRepository.GetByIdAsync(employeeId);

            if (employee == null)
            {
                return null;
            }

            Department? department = null;

            if (!string.IsNullOrWhiteSpace(employee.DepartmentId))
            {
                department = await _departmentRepository
                    .GetByIdAsync(employee.DepartmentId);
            }

            var firstDay = new DateTime(year, month, 1);
            var lastDay = firstDay.AddMonths(1).AddDays(-1);

            // Read one extra day either side so that no record is lost
            // because of the UTC <-> local time conversion, then keep
            // only this employee's records that fall inside the month.
            var rangeRecords = await _attendanceRepository
                .GetByDateRangeAsync(firstDay.AddDays(-1), lastDay.AddDays(1));

            var recordsByDate = rangeRecords
                .Where(a => a.EmployeeId == employeeId)
                .GroupBy(a => ToLocal(a.AttendanceDate).Date)
                .Where(g => g.Key >= firstDay && g.Key <= lastDay)
                .ToDictionary(
                    g => g.Key,
                    g => g
                        .OrderByDescending(a => a.ClockInTime.HasValue)
                        .First());

            var holidays = GetSouthAfricanPublicHolidays(year);

            var report = new AttendanceReportViewModel
            {
                EmployeeId = employee.Id,
                EmployeeName = $"{employee.FirstName} {employee.LastName}".Trim(),
                EmployeeNumber = employee.EmployeeNumber,
                DepartmentName = department?.DepartmentName ?? "-",
                Year = year,
                Month = month,
                StandardDailyHours = StandardDailyHours,
                GeneratedOn = DateTime.Now
            };

            double runningWeekHours = 0;

            for (var date = firstDay; date <= lastDay; date = date.AddDays(1))
            {
                recordsByDate.TryGetValue(date, out var record);

                var day = BuildDay(date, record, holidays);

                // "Each Week" column: weeks run Monday - Sunday.
                runningWeekHours += day.HoursWorked;

                if (date.DayOfWeek == DayOfWeek.Sunday || date == lastDay)
                {
                    day.WeekTotalHours = Round(runningWeekHours);
                    runningWeekHours = 0;
                }

                report.Days.Add(day);
            }

            // ----- Monthly totals -----
            report.TotalHoursWorked = Round(report.Days.Sum(d => d.HoursWorked));
            report.TotalOvertimeHours = Round(report.Days.Sum(d => d.OvertimeHours));
            report.TotalSundayHours = Round(report.Days.Sum(d => d.SundayHours));
            report.TotalPublicHolidayHours = Round(report.Days.Sum(d => d.PublicHolidayHours));

            var records = recordsByDate.Values.ToList();

            report.DaysPresent = records.Count(r => r.ClockInTime != null);
            report.DaysLate = records.Count(r => r.AttendanceStatus == AttendanceStatus.Late);
            report.DaysOnLeave = records.Count(r => r.AttendanceStatus == AttendanceStatus.OnLeave);
            report.DaysAbsent = report.Days.Count(d => d.Remarks == "Absent");

            return report;
        }

        public string BuildCsv(AttendanceReportViewModel report)
        {
            var sb = new StringBuilder();

            sb.AppendLine("Attendance Register");
            sb.AppendLine($"Employee Name:,{Csv(report.EmployeeName)},,Employee No:,{Csv(report.EmployeeNumber)}");
            sb.AppendLine($"Department:,{Csv(report.DepartmentName)}");
            sb.AppendLine($"Year:,{report.Year},,Month:,{report.MonthName}");
            sb.AppendLine();

            sb.AppendLine(
                "Day,Day of the Week,Starting Time,Meal Interval Off,Meal Interval On," +
                "Finishing Time,Hours Worked (Each Day),Hours Worked (Each Week)," +
                "Overtime Worked,Sundays Worked,Public Holidays Worked,Remarks,Signature");

            foreach (var d in report.Days)
            {
                sb.AppendLine(string.Join(",",
                    d.DayNumber,
                    d.DayOfWeekName,
                    Time(d.StartingTime),
                    Time(d.MealOff),
                    Time(d.MealOn),
                    Time(d.FinishingTime),
                    Hours(d.HoursWorked, d.HasRecord),
                    d.WeekTotalHours.HasValue ? Hours(d.WeekTotalHours.Value, true) : "",
                    Hours(d.OvertimeHours, d.OvertimeHours > 0),
                    Hours(d.SundayHours, d.SundayHours > 0),
                    Hours(d.PublicHolidayHours, d.PublicHolidayHours > 0),
                    Csv(d.Remarks),
                    ""));
            }

            sb.AppendLine(string.Join(",",
                "Total Number of Hours Worked for month:",
                "", "", "", "", "",
                Hours(report.TotalHoursWorked, true),
                "",
                Hours(report.TotalOvertimeHours, true),
                Hours(report.TotalSundayHours, true),
                Hours(report.TotalPublicHolidayHours, true),
                "",
                ""));

            sb.AppendLine();
            sb.AppendLine($"Days Present:,{report.DaysPresent}");
            sb.AppendLine($"Days Late:,{report.DaysLate}");
            sb.AppendLine($"Days Absent:,{report.DaysAbsent}");
            sb.AppendLine($"Days On Leave:,{report.DaysOnLeave}");
            sb.AppendLine($"Overtime rule:,Hours above {report.StandardDailyHours:0.##} per day (Mon - Sat)");
            sb.AppendLine($"Generated:,{report.GeneratedOn:yyyy/MM/dd HH:mm}");
            sb.AppendLine();
            sb.AppendLine("Employee Signature:,,,Manager Signature:");

            return sb.ToString();
        }

        // ------------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------------

        private static AttendanceReportDayViewModel BuildDay(
            DateTime date,
            Attendance? record,
            Dictionary<DateTime, string> holidays)
        {
            var day = new AttendanceReportDayViewModel
            {
                Date = date,
                HasRecord = record != null
            };

            if (holidays.TryGetValue(date, out var holidayName))
            {
                day.IsPublicHoliday = true;
                day.PublicHolidayName = holidayName;
            }

            if (record == null)
            {
                day.Remarks = GetRemarksWithoutRecord(day);
                return day;
            }

            day.StartingTime = ToLocal(record.ClockInTime);
            day.FinishingTime = ToLocal(record.ClockOutTime);

            var breaks = record.Breaks
                .OrderBy(b => b.StartTime)
                .ToList();

            day.BreakCount = breaks.Count;
            day.BreakMinutes = breaks.Sum(b => b.DurationMinutes);

            var firstBreak = breaks.FirstOrDefault();
            day.MealOff = ToLocal(firstBreak?.StartTime);
            day.MealOn = ToLocal(firstBreak?.EndTime);

            // Hours only count once the employee has clocked out.
            if (record.ClockInTime != null && record.ClockOutTime != null)
            {
                day.HoursWorked = Round(record.WorkingMinutes / 60.0);
            }

            if (day.IsPublicHoliday)
            {
                day.PublicHolidayHours = day.HoursWorked;
            }
            else if (day.IsSunday)
            {
                day.SundayHours = day.HoursWorked;
            }
            else
            {
                day.OvertimeHours = Round(
                    Math.Max(0, day.HoursWorked - StandardDailyHours));
            }

            var remarks = new List<string>();

            if (day.IsPublicHoliday)
            {
                remarks.Add(day.PublicHolidayName!);
            }

            remarks.Add(record.AttendanceStatus switch
            {
                AttendanceStatus.OnLeave => "On Leave",
                AttendanceStatus.HalfDay => "Half Day",
                _ => record.AttendanceStatus.ToString()
            });

            if (record.ClockInTime != null && record.ClockOutTime == null)
            {
                remarks.Add(date == DateTime.Today ? "In progress" : "Not clocked out");
            }

            if (breaks.Count > 1)
            {
                remarks.Add($"{breaks.Count} breaks ({day.BreakMinutes} min)");
            }

            day.Remarks = string.Join(" - ", remarks);

            return day;
        }

        private static string GetRemarksWithoutRecord(AttendanceReportDayViewModel day)
        {
            if (day.IsPublicHoliday)
            {
                return day.PublicHolidayName!;
            }

            if (day.IsSaturday || day.IsSunday)
            {
                return "Weekend";
            }

            // Days that have not happened yet stay blank.
            if (day.Date >= DateTime.Today)
            {
                return string.Empty;
            }

            return "Absent";
        }

        /// <summary>
        /// MongoDB returns dates in UTC - convert them to local time.
        /// </summary>
        private static DateTime ToLocal(DateTime value)
        {
            return value.Kind == DateTimeKind.Utc
                ? value.ToLocalTime()
                : value;
        }

        private static DateTime? ToLocal(DateTime? value)
        {
            return value.HasValue ? ToLocal(value.Value) : null;
        }

        private static double Round(double value)
        {
            return Math.Round(value, 2, MidpointRounding.AwayFromZero);
        }

        private static string Time(DateTime? value)
        {
            return value.HasValue ? value.Value.ToString("HH:mm") : "";
        }

        private static string Hours(double value, bool show)
        {
            return show ? value.ToString("0.00", CultureInfo.InvariantCulture) : "";
        }

        private static string Csv(string? value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return "";
            }

            if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            {
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            }

            return value;
        }

        /// <summary>
        /// South African public holidays for a year (Public Holidays Act).
        /// A holiday that falls on a Sunday moves to the Monday.
        /// </summary>
        private static Dictionary<DateTime, string> GetSouthAfricanPublicHolidays(int year)
        {
            var easterSunday = GetEasterSunday(year);

            var fixedHolidays = new Dictionary<DateTime, string>
            {
                [new DateTime(year, 1, 1)] = "New Year's Day",
                [new DateTime(year, 3, 21)] = "Human Rights Day",
                [easterSunday.AddDays(-2)] = "Good Friday",
                [easterSunday.AddDays(1)] = "Family Day",
                [new DateTime(year, 4, 27)] = "Freedom Day",
                [new DateTime(year, 5, 1)] = "Workers' Day",
                [new DateTime(year, 6, 16)] = "Youth Day",
                [new DateTime(year, 8, 9)] = "National Women's Day",
                [new DateTime(year, 9, 24)] = "Heritage Day",
                [new DateTime(year, 12, 16)] = "Day of Reconciliation",
                [new DateTime(year, 12, 25)] = "Christmas Day",
                [new DateTime(year, 12, 26)] = "Day of Goodwill"
            };

            var holidays = new Dictionary<DateTime, string>(fixedHolidays);

            foreach (var holiday in fixedHolidays)
            {
                if (holiday.Key.DayOfWeek == DayOfWeek.Sunday)
                {
                    var monday = holiday.Key.AddDays(1);

                    if (!holidays.ContainsKey(monday))
                    {
                        holidays[monday] = $"{holiday.Value} (observed)";
                    }
                }
            }

            return holidays;
        }

        /// <summary>
        /// Easter Sunday (Gregorian calendar - Anonymous algorithm).
        /// </summary>
        private static DateTime GetEasterSunday(int year)
        {
            int a = year % 19;
            int b = year / 100;
            int c = year % 100;
            int d = b / 4;
            int e = b % 4;
            int f = (b + 8) / 25;
            int g = (b - f + 1) / 3;
            int h = (19 * a + b - d - g + 15) % 30;
            int i = c / 4;
            int k = c % 4;
            int l = (32 + 2 * e + 2 * i - h - k) % 7;
            int m = (a + 11 * h + 22 * l) / 451;
            int month = (h + l - 7 * m + 114) / 31;
            int day = ((h + l - 7 * m + 114) % 31) + 1;

            return new DateTime(year, month, day);
        }
    }
}