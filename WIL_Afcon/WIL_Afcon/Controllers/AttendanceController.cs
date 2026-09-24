using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WIL_Afcon.Models;
using WIL_Afcon.Services.Interfaces;

namespace WIL_Afcon.Controllers
{
    [Authorize]
    public class AttendanceController : Controller
    {
        private readonly IAttendanceService _attendanceService;
        private readonly IEmployeeService _employeeService;

        public AttendanceController(
           IAttendanceService attendanceService,
           IEmployeeService employeeService)
        {
            _attendanceService = attendanceService;
            _employeeService = employeeService;
        }

        // GET: Attendance  (Admin – all employees, filtered by date)
        [Authorize(Roles = "Admin")]
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

        private async Task<Employee?> GetCurrentEmployeeAsync()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return null;
            }

            return await _employeeService.GetByUserIdAsync(userId);
        }
    }
}