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

       // Reference: Microsoft (2026) 'Dependency injection into controllers in ASP.NET Core',
// Microsoft Learn.
// Available at: https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/dependency-injection
// (Accessed: 26 September 2026).
        public EmployeeController(
            IEmployeeService employeeService,
            IDepartmentService departmentService)
        {
            _employeeService = employeeService;
            _departmentService = departmentService;
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

            await _employeeService.CreateAsync(employee);

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
                return View(employee);
            }

            await _employeeService.UpdateAsync(employee);

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
