using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WIL_Afcon.Models;
using WIL_Afcon.Services.Interfaces;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace WIL_Afcon.Controllers
{
    [Authorize]
    public class AttendanceRequestController : Controller
    {
        private readonly IAttendanceRequestService _attendanceRequestService;
        private readonly IEmployeeService _employeeService;

        public AttendanceRequestController(
            IAttendanceRequestService attendanceRequestService,
            IEmployeeService employeeService)
        {
            _attendanceRequestService = attendanceRequestService;
            _employeeService = employeeService;
        }


        // This is the ADMIN side 


        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var requests =
                await _attendanceRequestService.GetAllAsync();

            foreach (var request in requests)
            {
                var employee =
                    await _employeeService.GetByIdAsync(request.EmployeeId);

                if (employee != null)
                {
                    request.EmployeeName =
                        $"{employee.FirstName} {employee.LastName}";
                }
                else
                {
                    request.EmployeeName = "Unknown Employee";
                }
            }

            return View(requests);
        }



        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> DownloadDocument(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            var request =
                await _attendanceRequestService.GetByIdAsync(id);

            if (request == null)
            {
                return NotFound();
            }

            // Employees can only download documents
            // belonging to their own requests.
            if (User.IsInRole("Employee"))
            {
                var userId = User.FindFirst(
                    ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrWhiteSpace(userId))
                {
                    return Unauthorized();
                }

                var employee =
                    await _employeeService.GetByUserIdAsync(userId);

                if (employee == null)
                {
                    return NotFound(
                        "Employee profile could not be found.");
                }

                if (request.EmployeeId != employee.Id)
                {
                    return Forbid();
                }
            }

            // Admins can download documents from
            // attendance requests they can review.
            if (string.IsNullOrWhiteSpace(request.DocumentId))
            {
                return NotFound(
                    "No supporting document is attached.");
            }

            var document =
                await _attendanceRequestService
                    .DownloadDocumentAsync(request.DocumentId);

            if (document == null)
            {
                return NotFound(
                    "Supporting document could not be found.");
            }

            return File(
                document.Value.Stream,
                document.Value.ContentType,
                document.Value.FileName);
        }


        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            var request =
                await _attendanceRequestService.GetByIdAsync(id);

            if (request == null)
            {
                return NotFound();
            }

            return View(request);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Accept(
            string id,
            string? adminComment)
        {
            var success =
                await _attendanceRequestService.AcceptAsync(
                    id,
                    adminComment);

            if (!success)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(
            string id,
            string? adminComment)
        {
            var success =
                await _attendanceRequestService.RejectAsync(
                    id,
                    adminComment);

            if (!success)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Index));
        }


        
        // This is the employee side 
     

        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> MyRequests()
        {
            var userId = User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            var employee =
                await _employeeService.GetByUserIdAsync(userId);

            if (employee == null)
            {
                return NotFound(
                    "Employee profile could not be found.");
            }

            var requests =
                await _attendanceRequestService
                    .GetByEmployeeIdAsync(employee.Id);

            return View(requests);
        }

        [HttpGet]
        [Authorize(Roles = "Employee")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Employee")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            AttendanceRequest request, IFormFile? file)
        {
            if (!ModelState.IsValid)
            {
                return View(request);
            }

            var userId = User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            var employee =
                await _employeeService.GetByUserIdAsync(userId);

            if (employee == null)
            {
                ModelState.AddModelError(
                    "",
                    "Your employee profile could not be found.");

                return View(request);
            }

            // Automatically associate the request
            // with the logged-in employee.
            request.EmployeeId = employee.Id;

            var created =
                await _attendanceRequestService
                    .CreateAsync(request,file);

            if (created == null)
            {
                ModelState.AddModelError(
                    "",
                    "Unable to create attendance request.");

                return View(request);
            }

            return RedirectToAction(nameof(MyRequests));
        }



        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> MyDetails(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            var userId = User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            var employee =
                await _employeeService.GetByUserIdAsync(userId);

            if (employee == null)
            {
                return NotFound(
                    "Employee profile could not be found.");
            }

            var request =
                await _attendanceRequestService.GetByIdAsync(id);

            if (request == null)
            {
                return NotFound();
            }

            // Make sure the employee can only view
            // their own attendance request.
            if (request.EmployeeId != employee.Id)
            {
                return Forbid();
            }

            return View("Details", request);
        }
    }
}