using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WIL_Afcon.Models;
using WIL_Afcon.Models.ViewModels;
using WIL_Afcon.Services.Interfaces;

namespace WIL_Afcon.Controllers
{
    [Authorize]
    public class AttendanceReportController : Controller
    {
        private readonly IAttendanceReportService _reportService;
        private readonly IEmployeeService _employeeService;

        public AttendanceReportController(
            IAttendanceReportService reportService,
            IEmployeeService employeeService)
        {
            _reportService = reportService;
            _employeeService = employeeService;
        }

        // GET: /AttendanceReport  (Admin – pick any employee and month)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index(string? employeeId, int? year, int? month)
        {
            var (selectedYear, selectedMonth) = ResolvePeriod(year, month);

            await LoadFilterDataAsync(employeeId, selectedYear, selectedMonth);

            if (string.IsNullOrWhiteSpace(employeeId))
            {
                // No employee chosen yet – just show the filter form.
                return View((AttendanceReportViewModel?)null);
            }

            var report = await _reportService
                .GenerateMonthlyReportAsync(employeeId, selectedYear, selectedMonth);

            if (report == null)
            {
                ViewBag.Error = "The selected employee could not be found.";
            }

            return View(report);
        }

        // GET: /AttendanceReport/DownloadCsv  (Admin)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DownloadCsv(string employeeId, int year, int month)
        {
            var (selectedYear, selectedMonth) = ResolvePeriod(year, month);

            var report = await _reportService
                .GenerateMonthlyReportAsync(employeeId, selectedYear, selectedMonth);

            if (report == null)
            {
                return NotFound("Employee record could not be found.");
            }

            return CsvFile(report);
        }

        // GET: /AttendanceReport/MyReport  (Employee – own report only)
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> MyReport(int? year, int? month)
        {
            var employee = await GetCurrentEmployeeAsync();

            if (employee == null)
            {
                return NotFound("Employee record could not be found.");
            }

            var (selectedYear, selectedMonth) = ResolvePeriod(year, month);

            ViewBag.Years = BuildYearList(selectedYear);
            ViewBag.Months = BuildMonthList(selectedMonth);

            var report = await _reportService
                .GenerateMonthlyReportAsync(employee.Id, selectedYear, selectedMonth);

            return View(report);
        }

        // GET: /AttendanceReport/MyReportCsv  (Employee)
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> MyReportCsv(int year, int month)
        {
            var employee = await GetCurrentEmployeeAsync();

            if (employee == null)
            {
                return NotFound("Employee record could not be found.");
            }

            var (selectedYear, selectedMonth) = ResolvePeriod(year, month);

            var report = await _reportService
                .GenerateMonthlyReportAsync(employee.Id, selectedYear, selectedMonth);

            if (report == null)
            {
                return NotFound("Employee record could not be found.");
            }

            return CsvFile(report);
        }

        // ------------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------------

        private FileContentResult CsvFile(AttendanceReportViewModel report)
        {
            var csv = _reportService.BuildCsv(report);

            // UTF-8 with BOM so Excel shows special characters correctly.
            var bytes = Encoding.UTF8.GetPreamble()
                .Concat(Encoding.UTF8.GetBytes(csv))
                .ToArray();

            var safeNumber = string.IsNullOrWhiteSpace(report.EmployeeNumber)
                ? "Employee"
                : report.EmployeeNumber;

            var fileName =
                $"Attendance_Register_{safeNumber}{report.Year}{report.Month:00}.csv";

            return File(bytes, "text/csv", fileName);
        }

        private async Task LoadFilterDataAsync(string? employeeId, int year, int month)
        {
            var employees = await _employeeService.GetAllAsync();

            ViewBag.Employees = employees
                .OrderBy(e => e.FirstName)
                .ThenBy(e => e.LastName)
                .Select(e => new SelectListItem
                {
                    Value = e.Id,
                    Text = $"{e.FirstName} {e.LastName} ({e.EmployeeNumber})"
                           + (e.IsActive ? "" : " - inactive"),
                    Selected = e.Id == employeeId
                })
                .ToList();

            ViewBag.Years = BuildYearList(year);
            ViewBag.Months = BuildMonthList(month);
            ViewBag.SelectedEmployeeId = employeeId;
        }

        private static (int Year, int Month) ResolvePeriod(int? year, int? month)
        {
            var y = year is >= 2000 and <= 2100 ? year.Value : DateTime.Today.Year;
            var m = month is >= 1 and <= 12 ? month.Value : DateTime.Today.Month;

            return (y, m);
        }

        private static List<SelectListItem> BuildYearList(int selectedYear)
        {
            var currentYear = DateTime.Today.Year;

            return Enumerable.Range(currentYear - 5, 6)
                .OrderByDescending(y => y)
                .Select(y => new SelectListItem
                {
                    Value = y.ToString(),
                    Text = y.ToString(),
                    Selected = y == selectedYear
                })
                .ToList();
        }

        private static List<SelectListItem> BuildMonthList(int selectedMonth)
        {
            return Enumerable.Range(1, 12)
                .Select(m => new SelectListItem
                {
                    Value = m.ToString(),
                    Text = System.Globalization.CultureInfo.InvariantCulture
                        .DateTimeFormat.GetMonthName(m),
                    Selected = m == selectedMonth
                })
                .ToList();
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