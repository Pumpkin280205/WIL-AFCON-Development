using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WIL_Afcon.Models;
using WIL_Afcon.Services.Interfaces;

namespace WIL_Afcon.Controllers
{

// Reference: Microsoft (2026) 'Simple authorization in ASP.NET Core MVC',
// Microsoft Learn.
// Available at: https://learn.microsoft.com/en-us/aspnet/core/mvc/security/authorization/simple
// (Accessed: 26 September 2026).
    [Authorize]
    public class AttendanceController : Controller
    {
        private readonly IAttendanceService _attendanceService;
        private readonly IEmployeeService _employeeService;


// Reference: Microsoft (2026) 'Dependency injection into controllers in ASP.NET Core',
// Microsoft Learn.
// Available at: https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/dependency-injection
// (Accessed: 26 September 2026).
        public AttendanceController(
           IAttendanceService attendanceService,
           IEmployeeService employeeService)
        {
            _attendanceService = attendanceService;
            _employeeService = employeeService;
        }

        // GET: Attendance  (Admin – all employees, filtered by date)

        // Reference: Microsoft (2026) 'Role-based authorization in ASP.NET Core MVC',
// Microsoft Learn.
// Available at: https://learn.microsoft.com/en-us/aspnet/core/mvc/security/authorization/roles
// (Accessed: 26 September 2026).
        [Authorize(Roles = "Admin")]

        // Reference: Microsoft (2026) 'Model Binding in ASP.NET Core',
// Microsoft Learn.
// Available at: https://learn.microsoft.com/en-us/aspnet/core/mvc/models/model-binding
// (Accessed: 26 September 2026).
        public async Task<IActionResult> Index(DateTime? from, DateTime? to)
        {
            var fromDate = from ?? DateTime.Today.AddDays(-7);
            var toDate = to ?? DateTime.Today;

            var records = await _attendanceService
                .GetListForAdminAsync(fromDate, toDate);

            ViewBag.From = fromDate.ToString("yyyy-MM-dd");
            ViewBag.To = toDate.ToString("yyyy-MM-dd");

            return View(records);
        }

        // GET: /Attendance/MyAttendance  (Employee – own history incl. today)
        // Attendance is recorded ONLY by scanning on the Hikvision device.
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> MyAttendance()
        {
            var employee = await GetCurrentEmployeeAsync();

            if (employee == null)
            {
                return NotFound("Employee record could not be found.");
            }

            var records = await _attendanceService
                .GetByEmployeeIdAsync(employee.Id);

            ViewBag.Today = await _attendanceService
                .GetTodayForEmployeeAsync(employee.Id);

            return View(records
                .OrderByDescending(a => a.AttendanceDate)
                .ToList());
        }

        [HttpPost]
        [Authorize(Roles = "Employee")]

        // Reference: Microsoft (2026) 'Prevent Cross-Site Request Forgery (XSRF/CSRF) attacks
// in ASP.NET Core', Microsoft Learn.
// Available at: https://learn.microsoft.com/en-us/aspnet/core/security/anti-request-forgery
// (Accessed: 26 September 2026).
        [ValidateAntiForgeryToken]
        public Task<IActionResult> ClockIn()
            => RunEmployeeActionAsync(id => _attendanceService.ClockInAsync(id));

        [HttpPost]
        [Authorize(Roles = "Employee")]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> ClockOut()
           => RunEmployeeActionAsync(id => _attendanceService.ClockOutAsync(id));

        [HttpPost]
        [Authorize(Roles = "Employee")]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> StartBreak()
            => RunEmployeeActionAsync(id => _attendanceService.StartBreakAsync(id));

        [HttpPost]
        [Authorize(Roles = "Employee")]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> EndBreak()
            => RunEmployeeActionAsync(id => _attendanceService.EndBreakAsync(id));

        private async Task<IActionResult> RunEmployeeActionAsync(
            Func<string, Task> action)
        {
            var employee = await GetCurrentEmployeeAsync();

            if (employee == null)
            {
                return NotFound("Employee record could not be found.");
            }

            try
            {
                await action(employee.Id);
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(MyAttendance));
        }

        // ------------------------------------------------------------------
        // ADMIN MANUAL ATTENDANCE (backup for when the biometric device is
        // unavailable) - Admin records the action on behalf of a specific
        // employee, from that employee's Details page.
        // ------------------------------------------------------------------
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> AdminClockIn(string employeeId)
            => RunAdminActionAsync(employeeId, id => _attendanceService.ClockInAsync(id));

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> AdminClockOut(string employeeId)
            => RunAdminActionAsync(employeeId, id => _attendanceService.ClockOutAsync(id));

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> AdminStartBreak(string employeeId)
            => RunAdminActionAsync(employeeId, id => _attendanceService.StartBreakAsync(id));

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> AdminEndBreak(string employeeId)
            => RunAdminActionAsync(employeeId, id => _attendanceService.EndBreakAsync(id));

        private async Task<IActionResult> RunAdminActionAsync(
            string employeeId,
            Func<string, Task> action)
        {
            try
            {
                await action(employeeId);
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction("Details", "Employee", new { id = employeeId });
        }

        private async Task<Employee?> GetCurrentEmployeeAsync()
        {

        // Reference: Microsoft (2026) 'Claim-based authorization in ASP.NET Core MVC',
// Microsoft Learn.
// Available at: https://learn.microsoft.com/en-us/aspnet/core/mvc/security/authorization/claims
// (Accessed: 26 September 2026).
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return null;
            }

            return await _employeeService.GetByUserIdAsync(userId);
        }
    }
}


//Microsoft (2026) 'Claim-based authorization in ASP.NET Core MVC', Microsoft Learn. Available at: https://learn.microsoft.com/en-us/aspnet/core/mvc/security/authorization/claims (Accessed: 26 September 2026).

//Microsoft (2026) 'Model Binding in ASP.NET Core', Microsoft Learn. Available at: https://learn.microsoft.com/en-us/aspnet/core/mvc/models/model-binding (Accessed: 26 September 2026).

//Microsoft (2026) 'Role-based authorization in ASP.NET Core MVC', Microsoft Learn. Available at: https://learn.microsoft.com/en-us/aspnet/core/mvc/security/authorization/roles (Accessed: 26 September 2026).

//Microsoft (2026) 'Simple authorization in ASP.NET Core MVC', Microsoft Learn. Available at: https://learn.microsoft.com/en-us/aspnet/core/mvc/security/authorization/simple (Accessed: 26 September 2026).
