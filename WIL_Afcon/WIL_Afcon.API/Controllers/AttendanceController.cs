using Microsoft.AspNetCore.Mvc;
using WIL_Afcon.API.Models;
using WIL_Afcon.API.Services.Interfaces;

namespace WIL_Afcon.API.Controllers
{
    [ApiController]
    [Route("api/attendance")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public class AttendanceController : ControllerBase
    {
        private readonly IAttendanceService _attendanceService;

        public AttendanceController(IAttendanceService attendanceService)
        {
            _attendanceService = attendanceService;
        }

        // GET: api/attendance?from=2026-08-01&to=2026-09-01
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<Attendance>>> GetAll(DateTime? from, DateTime? to)
        {
            var fromDate = from ?? DateTime.Today.AddDays(-7);
            var toDate = to ?? DateTime.Today;

            var records = await _attendanceService.GetListForAdminAsync(fromDate, toDate);

            return Ok(records);
        }

        // GET: api/attendance/employee/{employeeId}
        [HttpGet("employee/{employeeId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
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
        [ProducesResponseType(typeof(Attendance), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public Task<IActionResult> ClockIn(string employeeId)
            => RunEmployeeActionAsync(employeeId, id => _attendanceService.ClockInAsync(id));

        // POST: api/attendance/{employeeId}/clock-out
        [HttpPost("{employeeId}/clock-out")]
        [ProducesResponseType(typeof(Attendance), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public Task<IActionResult> ClockOut(string employeeId)
            => RunEmployeeActionAsync(employeeId, id => _attendanceService.ClockOutAsync(id));

        // POST: api/attendance/{employeeId}/break/start
        [HttpPost("{employeeId}/break/start")]
        [ProducesResponseType(typeof(Attendance), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public Task<IActionResult> StartBreak(string employeeId)
            => RunEmployeeActionAsync(employeeId, id => _attendanceService.StartBreakAsync(id));

        // POST: api/attendance/{employeeId}/break/end
        [HttpPost("{employeeId}/break/end")]
        [ProducesResponseType(typeof(Attendance), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
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
                // Attendance rule broken (outside clock-in window, already on break...)
                return Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Attendance action not allowed");
            }
        }
    }
}