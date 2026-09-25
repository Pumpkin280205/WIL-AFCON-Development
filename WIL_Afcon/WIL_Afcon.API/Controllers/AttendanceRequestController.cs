using Microsoft.AspNetCore.Mvc;
using WIL_Afcon.API.Models;
using WIL_Afcon.API.Services.Interfaces;

namespace WIL_Afcon.API.Controllers
{
    [ApiController]
    [Route("api/attendance-requests")]
    public class AttendanceRequestController : ControllerBase
    {
        private readonly IAttendanceRequestService _attendanceRequestService;

        public AttendanceRequestController(
            IAttendanceRequestService attendanceRequestService)
        {
            _attendanceRequestService = attendanceRequestService;
        }

        // GET: api/attendance-requests
        [HttpGet]
        public async Task<ActionResult<List<AttendanceRequest>>> GetAll()
        {
            var requests = await _attendanceRequestService.GetAllAsync();

            return Ok(requests);
        }

        // GET: api/attendance-requests/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var request = await _attendanceRequestService.GetByIdAsync(id);

            if (request == null)
            {
                return NotFound(new
                {
                    error = "Attendance request not found."
                });
            }

            return Ok(request);
        }

        // GET: api/attendance-requests/employee/{employeeId}
        [HttpGet("employee/{employeeId}")]
        public async Task<IActionResult> GetByEmployee(string employeeId)
        {
            var requests =
                await _attendanceRequestService.GetByEmployeeIdAsync(employeeId);

            return Ok(requests);
        }

        // GET: api/attendance-requests/pending
        [HttpGet("pending")]
        public async Task<ActionResult<List<AttendanceRequest>>> GetPending()
        {
            var requests = await _attendanceRequestService.GetPendingAsync();

            return Ok(requests);
        }

        // POST: api/attendance-requests
        [HttpPost]
        public async Task<IActionResult> Create(AttendanceRequest request)
        {
            var created =
                await _attendanceRequestService.CreateAsync(request);

            return Ok(created);
        }

        // PUT: api/attendance-requests/{id}/accept
        [HttpPut("{id}/accept")]
        public async Task<IActionResult> Accept(
            string id,
            [FromBody] string? adminComment)
        {
            var success = await _attendanceRequestService.AcceptAsync(
                id,
                adminComment ?? string.Empty,
                "Admin");

            if (!success)
            {
                return NotFound(new
                {
                    error = "Attendance request not found."
                });
            }

            return Ok(new
            {
                message = "Attendance request accepted."
            });
        }

        // PUT: api/attendance-requests/{id}/reject
        [HttpPut("{id}/reject")]
        public async Task<IActionResult> Reject(
            string id,
            [FromBody] string? adminComment)
        {
            var success = await _attendanceRequestService.RejectAsync(
                id,
                adminComment ?? string.Empty,
                "Admin");

            if (!success)
            {
                return NotFound(new
                {
                    error = "Attendance request not found."
                });
            }

            return Ok(new
            {
                message = "Attendance request rejected."
            });
        }
    }
}