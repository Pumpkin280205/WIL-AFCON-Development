using Microsoft.AspNetCore.Mvc;
using WIL_Afcon.API.Models;
using WIL_Afcon.API.Security;
using WIL_Afcon.API.Services.Interfaces;
using Microsoft.AspNetCore.Http;

namespace WIL_Afcon.API.Controllers
{
    [ApiController]
    [Route("api/attendance-requests")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public class AttendanceRequestController : ControllerBase
    {
        private readonly IAttendanceRequestService _attendanceRequestService;

        private readonly IAttendanceDocumentService _attendanceDocumentService;

        //public AttendanceRequestController(
        //    IAttendanceRequestService attendanceRequestService)
        //{
        //    _attendanceRequestService = attendanceRequestService;
        //}

        public AttendanceRequestController(
    IAttendanceRequestService attendanceRequestService,
    IAttendanceDocumentService attendanceDocumentService)
        {
            _attendanceRequestService = attendanceRequestService;
            _attendanceDocumentService = attendanceDocumentService;
        }
        // GET: api/attendance-requests
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<AttendanceRequest>>> GetAll()
        {
            var requests = await _attendanceRequestService.GetAllAsync();

            return Ok(requests);
        }

        // GET: api/attendance-requests/{id}
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(AttendanceRequest), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(string id)
        {
            var request = await _attendanceRequestService.GetByIdAsync(id);

            if (request == null)
            {
                return Problem(
                    detail: "Attendance request not found.",
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Not found");
            }

            return Ok(request);
        }

        // GET: api/attendance-requests/employee/{employeeId}
        [HttpGet("employee/{employeeId}")]
        [ProducesResponseType(typeof(List<AttendanceRequest>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByEmployee(string employeeId)
        {
            var requests =
                await _attendanceRequestService.GetByEmployeeIdAsync(employeeId);

            return Ok(requests);
        }

        // GET: api/attendance-requests/pending
        [HttpGet("pending")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<AttendanceRequest>>> GetPending()
        {
            var requests = await _attendanceRequestService.GetPendingAsync();

            return Ok(requests);
        }

        // POST: api/attendance-requests
        //[HttpPost]
        //public async Task<IActionResult> Create(AttendanceRequest request)
        //{
        //    var created =
        //        await _attendanceRequestService.CreateAsync(request);

        //    return Ok(created);
        //}


        [HttpPost]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(AttendanceRequest), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create(
    [FromForm] AttendanceRequest request,
    IFormFile? file)
        {
            if (file != null && file.Length > 0)
            {
                var uploadError = FileUploadValidator.Validate(file);

                if (uploadError != null)
                {
                    return Problem(
                        detail: uploadError,
                        statusCode: StatusCodes.Status400BadRequest,
                        title: "Invalid upload");
                }

                var fileName = FileUploadValidator.SafeFileName(file.FileName);
                var contentType = FileUploadValidator.ContentTypeFor(fileName);

                await using var stream = file.OpenReadStream();

                var documentId =
                    await _attendanceDocumentService.UploadAsync(
                        stream,
                        fileName,
                        contentType);

                request.DocumentId = documentId;
                request.DocumentName = fileName;
                request.DocumentContentType = contentType;
            }

            var created =
                await _attendanceRequestService.CreateAsync(request);

            return Ok(created);
        }

        // PUT: api/attendance-requests/{id}/accept
        [HttpPut("{id}/accept")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
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
                return Problem(
                    detail: "Attendance request not found.",
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Not found");
            }

            return Ok(new
            {
                message = "Attendance request accepted."
            });
        }

        // PUT: api/attendance-requests/{id}/reject
        [HttpPut("{id}/reject")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
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
                return Problem(
                    detail: "Attendance request not found.",
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Not found");
            }

            return Ok(new
            {
                message = "Attendance request rejected."
            });
        }
    }
}