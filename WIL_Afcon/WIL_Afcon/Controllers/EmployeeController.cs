using Microsoft.AspNetCore.Mvc;
using WIL_Afcon.Models;
using WIL_Afcon.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace WIL_Afcon.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly IEmployeeService _employeeService;
        private readonly IDepartmentService _departmentService;
        private readonly IDocumentService _documentService;
        private readonly IAttendanceService _attendanceService;


       // Reference: Microsoft (2026) 'Dependency injection into controllers in ASP.NET Core',
// Microsoft Learn.
// Available at: https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/dependency-injection
// (Accessed: 26 September 2026).
        public EmployeeController(
            IEmployeeService employeeService,
            IDepartmentService departmentService,
            IDocumentService documentService,
            IAttendanceService attendanceService)
        {
            _employeeService = employeeService;
            _departmentService = departmentService;
            _documentService = documentService;
            _attendanceService = attendanceService;
        }

        // GET: /Employee

        // Reference: Microsoft (2026) 'Role-based authorization in ASP.NET Core MVC',
// Microsoft Learn.
// Available at: https://learn.microsoft.com/en-us/aspnet/core/mvc/security/authorization/roles
// (Accessed: 26 September 2026).
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var employees = await _employeeService.GetAllAsync();

            return View(employees);
        }

        // GET: /Employee/Details/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(string id)
        {
            var employee = await _employeeService.GetByIdAsync(id);

            if (employee == null)
            {
                return NotFound();
            }

            ViewBag.Documents = await _documentService.GetByEmployeeIdAsync(id);
            ViewBag.TodayAttendance = await _attendanceService.GetTodayForEmployeeAsync(id);

            return View(employee);
        }

        // GET: /Employee/Create
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            var departments = await _departmentService.GetAllAsync();

            ViewBag.Departments = departments;

            return View();
        }

        // POST: /Employee/Create

        [HttpPost]
        [Authorize(Roles = "Admin")]


        // Reference: Microsoft (2026) 'Prevent Cross-Site Request Forgery (XSRF/CSRF) attacks
// in ASP.NET Core', Microsoft Learn.
// Available at: https://learn.microsoft.com/en-us/aspnet/core/security/anti-request-forgery
// (Accessed: 26 September 2026).
        [ValidateAntiForgeryToken]

        // Reference: Microsoft (2026) 'Model Binding in ASP.NET Core',
// Microsoft Learn.
// Available at: https://learn.microsoft.com/en-us/aspnet/core/mvc/models/model-binding
// (Accessed: 26 September 2026).
        public async Task<IActionResult> Create(Employee employee)
        {

     // Reference: Microsoft (2026) 'Model validation in ASP.NET Core MVC',
// Microsoft Learn.
// Available at: https://learn.microsoft.com/en-us/aspnet/core/mvc/models/validation
// (Accessed: 26 September 2026).   
            if (!ModelState.IsValid)
            {
                var departments = await _departmentService.GetAllAsync();
                ViewBag.Departments = departments;

                return View(employee);
            }

            try
            {
                await _employeeService.CreateAsync(employee);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(nameof(Employee.DeviceUserId), ex.Message);

                ViewBag.Departments = await _departmentService.GetAllAsync();

                return View(employee);
            }

            TempData["Success"] = $"{employee.FirstName} {employee.LastName} was added.";

            return RedirectToAction(nameof(Index));
        }

        // GET: /Employee/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(string id)
        {
            var employee = await _employeeService.GetByIdAsync(id);

            if (employee == null)
            {
                return NotFound();
            }

            ViewBag.Departments = await _departmentService.GetAllAsync();

            return View(employee);
        }

        // POST: /Employee/Edit
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Employee employee)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Departments = await _departmentService.GetAllAsync();
                return View(employee);
            }

            try
            {
                await _employeeService.UpdateAsync(employee);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(nameof(Employee.DeviceUserId), ex.Message);

                ViewBag.Departments = await _departmentService.GetAllAsync();

                return View(employee);
            }

            TempData["Success"] = $"Changes to {employee.FirstName} {employee.LastName} were saved.";

            return RedirectToAction(nameof(Index));
        }

        // POST: /Employee/Deactivate
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(string id)
        {
            await _employeeService.DeactivateAsync(id);

            return RedirectToAction(nameof(Index));
        }


        // GET: /Employee/Dashboard
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> Dashboard()
        {

        // Reference: Microsoft (2026) 'Claim-based authorization in ASP.NET Core MVC',
// Microsoft Learn.
// Available at: https://learn.microsoft.com/en-us/aspnet/core/mvc/security/authorization/claims
// (Accessed: 26 September 2026).
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var employee = await _employeeService.GetByUserIdAsync(userId);

            if (employee == null)
            {
                return NotFound("Employee record could not be found.");
            }

            return View(employee);
        }

        // GET: /Employee/Profile
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> Profile()
        {
            var userId = User.FindFirst(
                System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var employee = await _employeeService.GetByUserIdAsync(userId);

            if (employee == null)
            {
                return NotFound("Employee record could not be found.");
            }

            var department = await _departmentService.GetByIdAsync(
                employee.DepartmentId);

            ViewBag.DepartmentName =
                department?.DepartmentName ?? "Not assigned";

            return View(employee);
        }


    }
}


//References

//Microsoft (2026) 'Claim-based authorization in ASP.NET Core MVC', Microsoft Learn.
//Available at: https://learn.microsoft.com/en-us/aspnet/core/mvc/security/authorization/claims
//(Accessed: 26 September 2026).

//Microsoft (2026) 'Dependency injection into controllers in ASP.NET Core', Microsoft Learn.
//Available at: https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/dependency-injection
//(Accessed: 26 September 2026).

//Microsoft (2026) 'Model Binding in ASP.NET Core', Microsoft Learn.
//Available at: https://learn.microsoft.com/en-us/aspnet/core/mvc/models/model-binding
//(Accessed: 26 September 2026).

//Microsoft (2026) 'Model validation in ASP.NET Core MVC', Microsoft Learn.
//Available at: https://learn.microsoft.com/en-us/aspnet/core/mvc/models/validation
//(Accessed: 26 September 2026).

//Microsoft (2026) 'Prevent Cross-Site Request Forgery (XSRF/CSRF) attacks in ASP.NET Core', Microsoft Learn.
//Available at: https://learn.microsoft.com/en-us/aspnet/core/security/anti-request-forgery
//(Accessed: 26 September 2026).

//Microsoft (2026) 'Role-based authorization in ASP.NET Core MVC', Microsoft Learn.
//Available at: https://learn.microsoft.com/en-us/aspnet/core/mvc/security/authorization/roles
//(Accessed: 26 September 2026).