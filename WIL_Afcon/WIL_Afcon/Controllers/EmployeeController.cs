using Microsoft.AspNetCore.Mvc;
using WIL_Afcon.Models;
using WIL_Afcon.Services.Interfaces;

namespace WIL_Afcon.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly IEmployeeService _employeeService;
        private readonly IDepartmentService _departmentService;

        public EmployeeController(
            IEmployeeService employeeService,
            IDepartmentService departmentService)
        {
            _employeeService = employeeService;
            _departmentService = departmentService;
        }

        // GET: /Employee
        public async Task<IActionResult> Index()
        {
            var employees = await _employeeService.GetAllAsync();

            return View(employees);
        }

        // GET: /Employee/Details/5
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
        public async Task<IActionResult> Create()
        {
            var departments = await _departmentService.GetAllAsync();

            ViewBag.Departments = departments;

            return View();
        }

        // POST: /Employee/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Employee employee)
        {
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(string id)
        {
            await _employeeService.DeactivateAsync(id);

            return RedirectToAction(nameof(Index));
        }
    }
}