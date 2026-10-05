using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WIL_Afcon.API.Security;
using WIL_Afcon.API.Services.Interfaces;

namespace WIL_Afcon.API.Controllers
{
    [ApiController]
    [Route("api/attendance-documents")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public class AttendanceDocumentController : ControllerBase
    {
        private readonly IAttendanceDocumentService
            _attendanceDocumentService;

        public AttendanceDocumentController(
            IAttendanceDocumentService attendanceDocumentService)
        {
            _attendanceDocumentService =
                attendanceDocumentService;
        }

        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return Problem(
                    detail: "A file is required.",
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Invalid upload");
            }

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

            return Ok(new
            {
                documentId,
                fileName,
                contentType
            });
        }


        [HttpGet("{documentId}")]
        [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Download(
            string documentId)
        {
            var document =
                await _attendanceDocumentService
                    .DownloadAsync(documentId);

            if (document == null)
            {
                return Problem(
                    detail: "Document not found.",
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Not found");
            }

            return File(
                document.Value.Stream,
                document.Value.ContentType,
                document.Value.FileName);
        }
    }
}