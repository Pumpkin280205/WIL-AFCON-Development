using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WIL_Afcon.API.Security;
using WIL_Afcon.API.Services.Interfaces;

namespace WIL_Afcon.API.Controllers
{
    [ApiController]
    [Route("api/attendance-documents")]
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
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new
                {
                    error = "A file is required."
                });
            }

            var uploadError = FileUploadValidator.Validate(file);

            if (uploadError != null)
            {
                return BadRequest(new { error = uploadError });
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
        public async Task<IActionResult> Download(
            string documentId)
        {
            var document =
                await _attendanceDocumentService
                    .DownloadAsync(documentId);

            if (document == null)
            {
                return NotFound(new
                {
                    error = "Document not found."
                });
            }

            return File(
                document.Value.Stream,
                document.Value.ContentType,
                document.Value.FileName);
        }
    }
}