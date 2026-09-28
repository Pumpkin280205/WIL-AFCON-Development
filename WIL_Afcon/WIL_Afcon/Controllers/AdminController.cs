using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WIL_Afcon.Models.Enums;
using WIL_Afcon.Models.ViewModels;
using WIL_Afcon.Services.Interfaces;

namespace WIL_Afcon.Controllers
{
    // Reference: Microsoft (2026) 'Role-based authorization in ASP.NET Core MVC',
    // Microsoft Learn.
    // Available at: https://learn.microsoft.com/en-us/aspnet/core/mvc/security/authorization/roles
    // (Accessed: 26 September 2026).

    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IEmployeeService _employeeService;
        private readonly IAttendanceService _attendanceService;


        public AdminController(
            IEmployeeService employeeService,
            IAttendanceService attendanceService)
        {
            _employeeService = employeeService;
            _attendanceService = attendanceService;
        }


        // Reference: Microsoft (2026) 'IActionResult Interface',
        // Microsoft Learn.
        // Available at:
        // https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.iactionresult
        // (Accessed: 26 September 2026).

        public async Task<IActionResult> Dashboard()
        {
            var employees = await _employeeService.GetAllAsync();
            var attendanceRecords = await _attendanceService.GetAllAsync();

            var activeEmployees = employees
                .Where(e => e.IsActive)
                .ToList();

            var today = DateTime.Today;

            var todayAttendance = attendanceRecords
                .Where(a => a.AttendanceDate.Date == today)
                .ToList();


            // ==================================================
            // DASHBOARD TOTALS
            // ==================================================

            var model = new AdminDashboardViewModel
            {
                TotalEmployees = activeEmployees.Count,

                PresentToday = todayAttendance.Count(a =>
                    a.AttendanceStatus == AttendanceStatus.Present ||
                    a.AttendanceStatus == AttendanceStatus.Late ||
                    a.AttendanceStatus == AttendanceStatus.HalfDay),

                OnLeaveToday = todayAttendance.Count(a =>
                    a.AttendanceStatus == AttendanceStatus.OnLeave),

                AbsentToday = todayAttendance.Count(a =>
                    a.AttendanceStatus == AttendanceStatus.Absent)
            };


            // ==================================================
            // TODAY AT A GLANCE
            // ==================================================

            foreach (var attendance in todayAttendance)
            {
                var employee = activeEmployees.FirstOrDefault(
                    e => e.Id == attendance.EmployeeId);

                if (employee == null)
                {
                    continue;
                }

                model.TodayAttendance.Add(
                    new AdminTodayAttendanceViewModel
                    {
                        EmployeeNumber = employee.EmployeeNumber,
                        FirstName = employee.FirstName,
                        LastName = employee.LastName,
                        Status = attendance.AttendanceStatus.ToString()
                    });
            }


            // ==================================================
            // MONTHLY ATTENDANCE OVERVIEW
            // Last six months including the current month
            // ==================================================

            for (var monthsAgo = 5; monthsAgo >= 0; monthsAgo--)
            {
                var monthDate = today.AddMonths(-monthsAgo);

                var monthStart = new DateTime(
                    monthDate.Year,
                    monthDate.Month,
                    1);

                var monthEnd = monthStart.AddMonths(1);

                var monthRecords = attendanceRecords
                    .Where(a =>
                        a.AttendanceDate >= monthStart &&
                        a.AttendanceDate < monthEnd)
                    .ToList();

                model.MonthlyAttendance.Add(
                    new MonthlyAttendanceViewModel
                    {
                        Month = monthStart.ToString("MMM"),

                        Present = monthRecords.Count(a =>
                            a.AttendanceStatus == AttendanceStatus.Present),

                        Absent = monthRecords.Count(a =>
                            a.AttendanceStatus == AttendanceStatus.Absent),

                        Late = monthRecords.Count(a =>
                            a.AttendanceStatus == AttendanceStatus.Late)
                    });
            }


            return View(model);
        }
    }
}


// Microsoft (2026) 'IActionResult Interface', Microsoft Learn.
// Available at:
// https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.iactionresult
// (Accessed: 26 September 2026).

// Microsoft (2026) 'Role-based authorization in ASP.NET Core MVC',
// Microsoft Learn.
// Available at:
// https://learn.microsoft.com/en-us/aspnet/core/mvc/security/authorization/roles
// (Accessed: 26 September 2026).