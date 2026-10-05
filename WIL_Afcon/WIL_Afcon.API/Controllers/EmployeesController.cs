using Microsoft.AspNetCore.Mvc;
using WIL_Afcon.API.Models;
using WIL_Afcon.API.Models.Requests;
using WIL_Afcon.API.Services.Interfaces;

namespace WIL_Afcon.API.Controllers
{
    [ApiController]
    [Route("api/employees")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;

        public EmployeesController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        // GET: api/employees
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<Employee>>> GetAll()
        {
            var employees = await _employeeService.GetAllAsync();
            return Ok(employees);
        }

        // GET: api/employees/{id}
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Employee>> GetById(string id)
        {
            var employee = await _employeeService.GetByIdAsync(id);

            if (employee == null)
            {
                return EmployeeNotFound(id);
            }

            return Ok(employee);
        }

        // POST: api/employees
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<Employee>> Create([FromBody] Employee employee)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            await _employeeService.CreateAsync(employee);

            return CreatedAtAction(nameof(GetById), new { id = employee.Id }, employee);
        }

        // GET: api/employees/by-device/{deviceUserId}
        // Resolve which employee a biometric-terminal "Employee No." maps to.
        [HttpGet("by-device/{deviceUserId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Employee>> GetByDeviceUserId(string deviceUserId)
        {
            var employee = await _employeeService.GetByDeviceUserIdAsync(deviceUserId);

            return employee is null
                ? Problem(
                    detail: $"No employee is bound to device user '{deviceUserId}'.",
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Not found")
                : Ok(employee);
        }

        // PUT: api/employees/{id}/device-binding
        // Bind the terminal's "Employee No." to this employee (send null/empty to unbind).
        [HttpPut("{id}/device-binding")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<Employee>> UpdateDeviceBinding(
            string id,
            [FromBody] UpdateDeviceBindingRequest request)
        {
            try
            {
                var employee = await _employeeService.UpdateDeviceBindingAsync(id, request.DeviceUserId);

                return employee is null ? EmployeeNotFound(id) : Ok(employee);
            }
            catch (InvalidOperationException ex)
            {
                // Device user ID already bound to another employee
                return Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status409Conflict,
                    title: "Device user ID already in use");
            }
        }

        private ObjectResult EmployeeNotFound(string id)
            => Problem(
                detail: $"Employee '{id}' was not found.",
                statusCode: StatusCodes.Status404NotFound,
                title: "Not found");
    }
}