using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WIL_Afcon.Models.Enums;
using WIL_Afcon.Services.Interfaces;

namespace WIL_Afcon.Controllers
{
    [Authorize]
    public class DocumentController : Controller
    {
        private readonly IDocumentService _documentService;
        private readonly IEmployeeService _employeeService;

        public DocumentController(
            IDocumentService documentService,
            IEmployeeService employeeService)
        {
            _documentService = documentService;
            _employeeService = employeeService;
        }

        // GET: /Document/Upload/{employeeId}  (Admin – upload a payslip/IRP5 for one employee)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Upload(string employeeId)
        {
            var employee = await _employeeService.GetByIdAsync(employeeId);

            if (employee == null)
            {
                return NotFound("Employee record could not be found.");
            }

            ViewBag.Employee = employee;

            return View();
        }

        // POST: /Document/Upload
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(
            string employeeId,
            DocumentType documentType,
            IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError(string.Empty, "Please choose a file to upload.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Employee = await _employeeService.GetByIdAsync(employeeId);
                return View();
            }

            try
            {
                await using var stream = file!.OpenReadStream();

                await _documentService.UploadAsync(
                    employeeId,
                    documentType,
                    file.FileName,
                    file.ContentType,
                    stream);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                ViewBag.Employee = await _employeeService.GetByIdAsync(employeeId);
                return View();
            }

            return RedirectToAction("Details", "Employee", new { id = employeeId });
        }

        // GET: /Document/MyDocuments  (Employee – own payslips/IRP5s)
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> MyDocuments()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var employee = await _employeeService.GetByUserIdAsync(userId);

            if (employee == null)
            {
                return NotFound("Employee record could not be found.");
            }

            var documents = await _documentService.GetByEmployeeIdAsync(employee.Id);

            return View(documents);
        }

        // GET: /Document/Download/{id}  (either role, ownership checked below)
        public async Task<IActionResult> Download(string id)
        {
            var result = await _documentService.GetForDownloadAsync(id);

            if (result == null)
            {
                return NotFound();
            }

            var (document, fileStream) = result.Value;

            if (!User.IsInRole("Admin"))
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var employee = string.IsNullOrEmpty(userId)
                    ? null
                    : await _employeeService.GetByUserIdAsync(userId);

                if (employee == null || employee.Id != document.EmployeeId)
                {
                    return Forbid();
                }
            }

            return File(fileStream, document.ContentType, document.FileName);
        }
    }
}
