using WIL_Afcon.Models;
using WIL_Afcon.Repositories.Interfaces;
using WIL_Afcon.Services.Interfaces;
using System.Net.Http.Json;

namespace WIL_Afcon.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly HttpClient _httpClient;

        public EmployeeService(
    IEmployeeRepository employeeRepository,
    HttpClient httpClient)
        {
            _employeeRepository = employeeRepository;
            _httpClient = httpClient;
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
            await EnsureDeviceUserIdIsAvailableAsync(employee.DeviceUserId, employee.Id);

            await _employeeRepository.CreateAsync(employee);
        }

        public async Task UpdateAsync(Employee employee)
        {
            await EnsureDeviceUserIdIsAvailableAsync(employee.DeviceUserId, employee.Id);

            await _employeeRepository.UpdateAsync(employee);
        }

        // ------------------------------------------------------------------
        // Device User IDs must be unique - two employees sharing one would
        // make the biometric terminal's scans ambiguous as to who clocked in.
        // ------------------------------------------------------------------
        private async Task EnsureDeviceUserIdIsAvailableAsync(
            string? deviceUserId,
            string employeeId)
        {
            if (string.IsNullOrWhiteSpace(deviceUserId))
            {
                return;
            }

            var existing = await _employeeRepository.GetByDeviceUserIdAsync(deviceUserId);

            if (existing != null && existing.Id != employeeId)
            {
                throw new InvalidOperationException(
                    $"Device User ID '{deviceUserId}' is already assigned to " +
                    $"{existing.FirstName} {existing.LastName}.");
            }
        }

        //public async Task DeactivateAsync(string id)
        //{
        //    var employee = await _employeeRepository.GetByIdAsync(id);

        //    if (employee == null)
        //        return;

        //    employee.IsActive = false;

        //    await _employeeRepository.UpdateAsync(employee);
        //}

        public async Task DeactivateAsync(string id)
        {
            var employee = await _employeeRepository.GetByIdAsync(id);

            if (employee == null)
                return;

            if (string.IsNullOrWhiteSpace(employee.DeviceUserId))
                return;

            var response = await _httpClient.DeleteAsync(
                $"api/attendance/device/employee/{Uri.EscapeDataString(employee.DeviceUserId)}");

            if (!response.IsSuccessStatusCode)
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