using WIL_Afcon.API.Models;
using WIL_Afcon.API.Repositories.Interfaces;
using WIL_Afcon.API.Services.Interfaces;

namespace WIL_Afcon.API.Services
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

        public async Task<Employee?> GetByUserIdAsync(string userId)
        {
            return await _employeeRepository.GetByUserIdAsync(userId);
        }

        public async Task<Employee?> GetByDeviceUserIdAsync(string deviceUserId)
        {
            return await _employeeRepository.GetByDeviceUserIdAsync(deviceUserId);
        }

        public async Task<Employee?> UpdateDeviceBindingAsync(string employeeId, string? deviceUserId)
        {
            var employee = await _employeeRepository.GetByIdAsync(employeeId);
            if (employee is null)
            {
                return null;
            }

            var normalised = string.IsNullOrWhiteSpace(deviceUserId)
                ? null
                : deviceUserId.Trim();

            if (normalised is not null)
            {
                var existing = await _employeeRepository.GetByDeviceUserIdAsync(normalised);
                if (existing is not null && existing.Id != employeeId)
                {
                    throw new InvalidOperationException(
                        $"Device user '{normalised}' is already bound to employee {existing.Id}.");
                }
            }

            employee.DeviceUserId = normalised;
            await _employeeRepository.UpdateAsync(employee);

            return employee;
        }
    }
}