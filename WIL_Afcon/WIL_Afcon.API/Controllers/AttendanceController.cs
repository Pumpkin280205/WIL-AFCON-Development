using Microsoft.AspNetCore.Mvc;
using WIL_Afcon.API.Models;
using WIL_Afcon.API.Services.Interfaces;

namespace WIL_Afcon.API.Controllers
{
    [ApiController]
    [Route("api/attendance")]
    public class AttendanceController : ControllerBase
    {
        private readonly IAttendanceService _attendanceService;

        public AttendanceController(IAttendanceService attendanceService)
        {
            _attendanceService = attendanceService;
        }

        // GET: api/attendance?from=2026-08-01&to=2026-09-01
        [HttpGet]
        public async Task<ActionResult<List<Attendance>>> GetAll(DateTime? from, DateTime? to)
        {
            var fromDate = from ?? DateTime.Today.AddDays(-7);
            var toDate = to ?? DateTime.Today;

            var records = await _attendanceService.GetListForAdminAsync(fromDate, toDate);

            return Ok(records);
        }

        // GET: api/attendance/employee/{employeeId}
        [HttpGet("employee/{employeeId}")]
        public async Task<IActionResult> GetByEmployee(string employeeId)
        {
            var records = await _attendanceService.GetByEmployeeIdAsync(employeeId);
            var today = await _attendanceService.GetTodayForEmployeeAsync(employeeId);

            return Ok(new
            {
                Today = today,
                History = records.OrderByDescending(a => a.AttendanceDate).ToList()
            });
        }

        // POST: api/attendance/{employeeId}/clock-in
        [HttpPost("{employeeId}/clock-in")]
        public Task<IActionResult> ClockIn(string employeeId)
            => RunEmployeeActionAsync(employeeId, id => _attendanceService.ClockInAsync(id));

        // POST: api/attendance/{employeeId}/clock-out
        [HttpPost("{employeeId}/clock-out")]
        public Task<IActionResult> ClockOut(string employeeId)
            => RunEmployeeActionAsync(employeeId, id => _attendanceService.ClockOutAsync(id));

        // POST: api/attendance/{employeeId}/break/start
        [HttpPost("{employeeId}/break/start")]
        public Task<IActionResult> StartBreak(string employeeId)
            => RunEmployeeActionAsync(employeeId, id => _attendanceService.StartBreakAsync(id));

        // POST: api/attendance/{employeeId}/break/end
        [HttpPost("{employeeId}/break/end")]
        public Task<IActionResult> EndBreak(string employeeId)
            => RunEmployeeActionAsync(employeeId, id => _attendanceService.EndBreakAsync(id));

        private async Task<IActionResult> RunEmployeeActionAsync(
            string employeeId,
            Func<string, Task<Attendance>> action)
        {
            try
            {
                var updated = await action(employeeId);
                return Ok(updated);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}