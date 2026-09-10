using WIL_Afcon.API.Models;

namespace WIL_Afcon.API.Services.Interfaces
{
    public interface IEmployeeService
    {
        Task<List<Employee>> GetAllAsync();
        Task<Employee?> GetByIdAsync(string id);
        Task CreateAsync(Employee employee);
        Task<Employee?> GetByUserIdAsync(string userId);
        Task<Employee?> GetByEmailAsync(string email);
        Task<Employee?> GetByDeviceUserIdAsync(string deviceUserId);

        /// <summary>
        /// Binds (or, when <paramref name="deviceUserId"/> is null/blank, unbinds) the
        /// biometric terminal's "Employee No." to an employee. Returns null if the
        /// employee does not exist; throws <see cref="InvalidOperationException"/> if
        /// the device user is already bound to a different employee.
        /// </summary>
        Task<Employee?> UpdateDeviceBindingAsync(string employeeId, string? deviceUserId);
    }
}