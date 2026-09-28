using Microsoft.AspNetCore.Mvc;
using WIL_Afcon.API.Models;
using WIL_Afcon.API.Models.Requests;
using WIL_Afcon.API.Services.Interfaces;

namespace WIL_Afcon.API.Controllers
{
    [ApiController]
    [Route("api/employees")]
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;

        public EmployeesController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        // GET: api/employees
        [HttpGet]
        public async Task<ActionResult<List<Employee>>> GetAll()
        {
            var employees = await _employeeService.GetAllAsync();
            return Ok(employees);
        }

        // GET: api/employees/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Employee>> GetById(string id)
        {
            var employee = await _employeeService.GetByIdAsync(id);

            if (employee == null)
            {
                return NotFound();
            }

            return Ok(employee);
        }

        // POST: api/employees
        [HttpPost]
        public async Task<ActionResult<Employee>> Create([FromBody] Employee employee)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _employeeService.CreateAsync(employee);

            return CreatedAtAction(nameof(GetById), new { id = employee.Id }, employee);
        }

        // GET: api/employees/by-device/{deviceUserId}
        // Resolve which employee a biometric-terminal "Employee No." maps to.
        [HttpGet("by-device/{deviceUserId}")]
        public async Task<ActionResult<Employee>> GetByDeviceUserId(string deviceUserId)
        {
            var employee = await _employeeService.GetByDeviceUserIdAsync(deviceUserId);

            return employee is null ? NotFound() : Ok(employee);
        }

        // PUT: api/employees/{id}/device-binding
        // Bind the terminal's "Employee No." to this employee (send null/empty to unbind).
        [HttpPut("{id}/device-binding")]
        public async Task<ActionResult<Employee>> UpdateDeviceBinding(
            string id,
            [FromBody] UpdateDeviceBindingRequest request)
        {
            try
            {
                var employee = await _employeeService.UpdateDeviceBindingAsync(id, request.DeviceUserId);

                return employee is null ? NotFound() : Ok(employee);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { error = ex.Message });
            }
        }
    }
}