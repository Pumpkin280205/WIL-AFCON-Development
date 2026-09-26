using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WIL_Afcon.Models;
using WIL_Afcon.Services.Interfaces;

namespace WIL_Afcon.Controllers
{
    public class DepartmentController : Controller
    {
        private readonly IDepartmentService _departmentService;

// Reference: Microsoft (2026) 'Dependency injection into controllers in ASP.NET Core',
// Microsoft Learn.
// Available at: https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/dependency-injection
// (Accessed: 26 September 2026).
        public DepartmentController(IDepartmentService departmentService)
        {
            _departmentService = departmentService;
        }

        // GET: /Department

        // Reference: Microsoft (2026) 'Role-based authorization in ASP.NET Core MVC',
// Microsoft Learn.
// Available at: https://learn.microsoft.com/en-us/aspnet/core/mvc/security/authorization/roles
// (Accessed: 26 September 2026).
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()

        {
            var departments = await _departmentService.GetAllAsync();

            return View(departments);
        }

        // GET: /Department/Details/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(string id)
        {
            var department = await _departmentService.GetByIdAsync(id);

            if (department == null)
            {
                return NotFound();
            }

            return View(department);
        }

        // GET: /Department/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Department/Create
        [HttpPost]
        [ValidateAntiForgeryToken]

        // Reference: Microsoft (2026) 'Model Binding in ASP.NET Core',
// Microsoft Learn.
// Available at: https://learn.microsoft.com/en-us/aspnet/core/mvc/models/model-binding
// (Accessed: 26 September 2026).
        public async Task<IActionResult> Create(Department department)
        {

        // Reference: Microsoft (2026) 'Model validation in ASP.NET Core MVC',
// Microsoft Learn.
// Available at: https://learn.microsoft.com/en-us/aspnet/core/mvc/models/validation
// (Accessed: 26 September 2026).
            if (!ModelState.IsValid)
            {
                return View(department);
            }

            await _departmentService.CreateAsync(department);

            return RedirectToAction(nameof(Index));
        }

        // GET: /Department/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(string id)
        {
            var department = await _departmentService.GetByIdAsync(id);

            if (department == null)
            {
                return NotFound();
            }

            return View(department);
        }

        // POST: /Department/Edit
        [HttpPost]

        // Reference: Microsoft (2026) 'Prevent Cross-Site Request Forgery (XSRF/CSRF) attacks
// in ASP.NET Core', Microsoft Learn.
// Available at: https://learn.microsoft.com/en-us/aspnet/core/security/anti-request-forgery
// (Accessed: 26 September 2026).
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Department department)
        {
            if (!ModelState.IsValid)
            {
                return View(department);
            }

            await _departmentService.UpdateAsync(department);

            return RedirectToAction(nameof(Index));
        }

        // POST: /Department/Deactivate 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(string id)
        {
            await _departmentService.DeactivateAsync(id);

            return RedirectToAction(nameof(Index));
        }
    }
}

