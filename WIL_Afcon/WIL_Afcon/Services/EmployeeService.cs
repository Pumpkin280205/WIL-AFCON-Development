using WIL_Afcon.Models;
using WIL_Afcon.Repositories.Interfaces;
using WIL_Afcon.Services.Interfaces;

namespace WIL_Afcon.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _employeeRepository;

        public EmployeeService(IEmployeeRepository employeeRepository)
        {
            _employeeRepository = employeeRepository;
        }

        public async Task<List<Employee>> GetAllAsync()
        {
            return await _employeeRepository.GetAllAsync();
        }

        public async Task<Employee?> GetByIdAsync(string id)
        {
            return await _employeeRepository.GetByIdAsync(id);
        }

        public async Task<Employee?> GetByEmailAsync(string email)
        {
            return await _employeeRepository.GetByEmailAsync(email);
        }

        public async Task CreateAsync(Employee employee)
        {
            await _employeeRepository.CreateAsync(employee);
        }

        public async Task UpdateAsync(Employee employee)
        {
            await _employeeRepository.UpdateAsync(employee);
        }

        public async Task DeactivateAsync(string id)
        {
            var employee = await _employeeRepository.GetByIdAsync(id);

            if (employee == null)
                return;

            employee.IsActive = false;

            await _employeeRepository.UpdateAsync(employee);
        }

        public async Task<Employee?> GetByUserIdAsync(string userId)
        {
            return await _employeeRepository.GetByUserIdAsync(userId);
        }
    }
}