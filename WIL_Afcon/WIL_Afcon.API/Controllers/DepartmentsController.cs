using Microsoft.AspNetCore.Mvc;
using WIL_Afcon.API.Models;
using WIL_Afcon.API.Services.Interfaces;

namespace WIL_Afcon.API.Controllers
{
    [ApiController]
    [Route("api/departments")]
    public class DepartmentsController : ControllerBase
    {
        private readonly IDepartmentService _departmentService;

        public DepartmentsController(IDepartmentService departmentService)
        {
            _departmentService = departmentService;
        }

        // GET: api/departments
        [HttpGet]
        public async Task<ActionResult<List<Department>>> GetAll()
        {
            var departments = await _departmentService.GetAllAsync();
            return Ok(departments);
        }

        // GET: api/departments/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Department>> GetById(string id)
        {
            var department = await _departmentService.GetByIdAsync(id);

            if (department == null)
            {
                return NotFound();
            }

            return Ok(department);
        }

        // POST: api/departments
        [HttpPost]
        public async Task<ActionResult<Department>> Create([FromBody] Department department)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _departmentService.CreateAsync(department);

            return CreatedAtAction(nameof(GetById), new { id = department.Id }, department);
        }
    }
}