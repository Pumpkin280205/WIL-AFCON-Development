using Microsoft.AspNetCore.Mvc;
using WIL_Afcon.API.Models;
using WIL_Afcon.API.Services.Interfaces;

namespace WIL_Afcon.API.Controllers
{
    [ApiController]
    [Route("api/departments")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public class DepartmentsController : ControllerBase
    {
        private readonly IDepartmentService _departmentService;

        public DepartmentsController(IDepartmentService departmentService)
        {
            _departmentService = departmentService;
        }

        // GET: api/departments
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<Department>>> GetAll()
        {
            var departments = await _departmentService.GetAllAsync();
            return Ok(departments);
        }

        // GET: api/departments/{id}
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Department>> GetById(string id)
        {
            var department = await _departmentService.GetByIdAsync(id);

            if (department == null)
            {
                return Problem(
                    detail: $"Department '{id}' was not found.",
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Not found");
            }

            return Ok(department);
        }

        // POST: api/departments
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Department>> Create([FromBody] Department department)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            await _departmentService.CreateAsync(department);

            return CreatedAtAction(nameof(GetById), new { id = department.Id }, department);
        }
    }
}