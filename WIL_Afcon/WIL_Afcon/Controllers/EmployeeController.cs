using Microsoft.AspNetCore.Mvc;
using WIL_Afcon.Models;
using WIL_Afcon.Models.Enums;
using WIL_Afcon.Models.ViewModels;
using WIL_Afcon.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace WIL_Afcon.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly IEmployeeService _employeeService;
        private readonly IDepartmentService _departmentService;
        private readonly IDocumentService _documentService;
<<<<<<< Updated upstream

=======
        private readonly IAttendanceService _attendanceService;
>>>>>>> Stashed changes


        // Reference: Microsoft (2026) 'Dependency injection into controllers in ASP.NET Core',
        // Microsoft Learn.
        // Available at:
        // https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/dependency-injection
        // (Accessed: 26 September 2026).
        public EmployeeController(
            IEmployeeService employeeService,
            IDepartmentService departmentService,
<<<<<<< Updated upstream
            IDocumentService documentService)
=======
            IDocumentService documentService,
            IAttendanceService attendanceService)
>>>>>>> Stashed changes
        {
            _employeeService = employeeService;
            _departmentService = departmentService;
            _documentService = documentService;
<<<<<<< Updated upstream
=======
            _attendanceService = attendanceService;
>>>>>>> Stashed changes
        }


        // ============================================================
        // ADMIN SIDE
        // ============================================================


        // GET: /Employee

        // Reference: Microsoft (2026) 'Role-based authorization in ASP.NET Core MVC',
        // Microsoft Learn.
        // Available at:
        // https://learn.microsoft.com/en-us/aspnet/core/mvc/security/authorization/roles
        // (Accessed: 26 September 2026).
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var employees =
                await _employeeService.GetAllAsync();

            return View(employees);
        }


        // GET: /Employee/Details/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(string id)
        {
            var employee =
                await _employeeService.GetByIdAsync(id);

            if (employee == null)
            {
                return NotFound();
            }

<<<<<<< Updated upstream
            ViewBag.Documents = await _documentService.GetByEmployeeIdAsync(id);
=======
            ViewBag.Documents =
                await _documentService.GetByEmployeeIdAsync(id);
>>>>>>> Stashed changes

            return View(employee);
        }


        // GET: /Employee/Create
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            var departments =
                await _departmentService.GetAllAsync();

            ViewBag.Departments = departments;

            return View();
        }


        // POST: /Employee/Create
        [HttpPost]
        [Authorize(Roles = "Admin")]

        // Reference: Microsoft (2026) 'Prevent Cross-Site Request Forgery (XSRF/CSRF) attacks
        // in ASP.NET Core', Microsoft Learn.
        // Available at:
        // https://learn.microsoft.com/en-us/aspnet/core/security/anti-request-forgery
        // (Accessed: 26 September 2026).
        [ValidateAntiForgeryToken]

        // Reference: Microsoft (2026) 'Model Binding in ASP.NET Core',
        // Microsoft Learn.
        // Available at:
        // https://learn.microsoft.com/en-us/aspnet/core/mvc/models/model-binding
        // (Accessed: 26 September 2026).
        public async Task<IActionResult> Create(Employee employee)
        {
            // Reference: Microsoft (2026) 'Model validation in ASP.NET Core MVC',
            // Microsoft Learn.
            // Available at:
            // https://learn.microsoft.com/en-us/aspnet/core/mvc/models/validation
            // (Accessed: 26 September 2026).
            if (!ModelState.IsValid)
            {
                var departments =
                    await _departmentService.GetAllAsync();

                ViewBag.Departments = departments;

                return View(employee);
            }

            await _employeeService.CreateAsync(employee);

            TempData["SuccessMessage"] =
                "Employee created successfully.";

            return RedirectToAction(nameof(Index));
        }


        // GET: /Employee/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(string id)
        {
            var employee =
                await _employeeService.GetByIdAsync(id);

            if (employee == null)
            {
                return NotFound();
            }

<<<<<<< Updated upstream
            ViewBag.Departments = await _departmentService.GetAllAsync();
=======
            ViewBag.Departments =
                await _departmentService.GetAllAsync();
>>>>>>> Stashed changes

            return View(employee);
        }


        // POST: /Employee/Edit
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Employee employee)
        {
            if (!ModelState.IsValid)
            {
<<<<<<< Updated upstream
                ViewBag.Departments = await _departmentService.GetAllAsync();
=======
                ViewBag.Departments =
                    await _departmentService.GetAllAsync();

>>>>>>> Stashed changes
                return View(employee);
            }

            await _employeeService.UpdateAsync(employee);

            TempData["SuccessMessage"] =
                "Employee updated successfully.";

            return RedirectToAction(nameof(Index));
        }


        // POST: /Employee/Deactivate
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(string id)
        {
            await _employeeService.DeactivateAsync(id);

            TempData["SuccessMessage"] =
                "Employee deactivated successfully.";

            return RedirectToAction(nameof(Index));
        }


        // ============================================================
        // EMPLOYEE SIDE
        // ============================================================


        // GET: /Employee/Dashboard
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> Dashboard()
        {
            // Reference: Microsoft (2026) 'Claim-based authorization in ASP.NET Core MVC',
            // Microsoft Learn.
            // Available at:
            // https://learn.microsoft.com/en-us/aspnet/core/mvc/security/authorization/claims
            // (Accessed: 26 September 2026).
            var userId = User.FindFirst(
                System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }


            // ========================================================
            // GET CURRENT EMPLOYEE
            // ========================================================

            var employee =
                await _employeeService.GetByUserIdAsync(userId);

            if (employee == null)
            {
                return NotFound(
                    "Employee record could not be found.");
            }


            // ========================================================
            // GET EMPLOYEE ATTENDANCE
            // ========================================================

            var attendanceRecords =
                await _attendanceService
                    .GetByEmployeeIdAsync(employee.Id);

            var today = DateTime.Today;


            // ========================================================
            // TODAY'S ATTENDANCE
            // ========================================================

            var todayAttendance =
                attendanceRecords.FirstOrDefault(a =>
                    a.AttendanceDate.Date == today);


            // ========================================================
            // CURRENT MONTH
            // ========================================================

            var monthStart =
                new DateTime(
                    today.Year,
                    today.Month,
                    1);

            var nextMonth =
                monthStart.AddMonths(1);

            var monthRecords =
                attendanceRecords
                    .Where(a =>
                        a.AttendanceDate >= monthStart &&
                        a.AttendanceDate < nextMonth)
                    .ToList();


            // ========================================================
            // CURRENT WEEK
            // Monday to Sunday
            // ========================================================

            var difference =
                (7 +
                 (today.DayOfWeek - DayOfWeek.Monday))
                % 7;

            var weekStart =
                today.AddDays(-difference);

            var weekEnd =
                weekStart.AddDays(7);

            var weekRecords =
                attendanceRecords
                    .Where(a =>
                        a.AttendanceDate.Date >=
                            weekStart.Date &&
                        a.AttendanceDate.Date <
                            weekEnd.Date)
                    .ToList();


            // ========================================================
            // WEEKLY HOURS
            // Monday to Friday
            // ========================================================

            var weeklyMinutes =
                new Dictionary<string, int>
                {
                    { "Mon", 0 },
                    { "Tue", 0 },
                    { "Wed", 0 },
                    { "Thu", 0 },
                    { "Fri", 0 }
                };

            foreach (var attendance in weekRecords)
            {
                var dayKey =
                    attendance.AttendanceDate.DayOfWeek
                    switch
                    {
                        DayOfWeek.Monday =>
                            "Mon",

                        DayOfWeek.Tuesday =>
                            "Tue",

                        DayOfWeek.Wednesday =>
                            "Wed",

                        DayOfWeek.Thursday =>
                            "Thu",

                        DayOfWeek.Friday =>
                            "Fri",

                        _ =>
                            null
                    };

                if (dayKey != null)
                {
                    weeklyMinutes[dayKey] +=
                        attendance.WorkingMinutes;
                }
            }


            // ========================================================
            // BUILD DASHBOARD
            // ========================================================

            var model =
                new EmployeeDashboardViewModel
                {
                    Employee = employee,

                    TodayAttendance =
                        todayAttendance,

                    RecentAttendance =
                        attendanceRecords
                            .OrderByDescending(a =>
                                a.AttendanceDate)
                            .Take(5)
                            .ToList(),

                    DaysPresent =
                        monthRecords.Count(a =>
                            a.AttendanceStatus ==
                                AttendanceStatus.Present ||

                            a.AttendanceStatus ==
                                AttendanceStatus.Late ||

                            a.AttendanceStatus ==
                                AttendanceStatus.HalfDay),

                    DaysAbsent =
                        monthRecords.Count(a =>
                            a.AttendanceStatus ==
                                AttendanceStatus.Absent),

                    LateArrivals =
                        monthRecords.Count(a =>
                            a.AttendanceStatus ==
                                AttendanceStatus.Late),

                    TotalMinutesThisMonth =
                        monthRecords.Sum(a =>
                            a.WorkingMinutes),

                    WeeklyMinutes =
                        weeklyMinutes
                };


            return View(model);
        }
    }
}


// ============================================================
// REFERENCES
// ============================================================

// Microsoft (2026) 'Claim-based authorization in ASP.NET Core MVC',
// Microsoft Learn.
// Available at:
// https://learn.microsoft.com/en-us/aspnet/core/mvc/security/authorization/claims
// (Accessed: 26 September 2026).

// Microsoft (2026) 'Dependency injection into controllers in ASP.NET Core',
// Microsoft Learn.
// Available at:
// https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/dependency-injection
// (Accessed: 26 September 2026).

// Microsoft (2026) 'Model Binding in ASP.NET Core',
// Microsoft Learn.
// Available at:
// https://learn.microsoft.com/en-us/aspnet/core/mvc/models/model-binding
// (Accessed: 26 September 2026).

// Microsoft (2026) 'Model validation in ASP.NET Core MVC',
// Microsoft Learn.
// Available at:
// https://learn.microsoft.com/en-us/aspnet/core/mvc/models/validation
// (Accessed: 26 September 2026).

// Microsoft (2026) 'Prevent Cross-Site Request Forgery (XSRF/CSRF)
// attacks in ASP.NET Core',
// Microsoft Learn.
// Available at:
// https://learn.microsoft.com/en-us/aspnet/core/security/anti-request-forgery
// (Accessed: 26 September 2026).

// Microsoft (2026) 'Role-based authorization in ASP.NET Core MVC',
// Microsoft Learn.
// Available at:
// https://learn.microsoft.com/en-us/aspnet/core/mvc/security/authorization/roles
// (Accessed: 26 September 2026).