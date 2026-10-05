using WIL_Afcon.Models;
using WIL_Afcon.Models.Enums;

namespace WIL_Afcon.Services.Interfaces
{
    public interface IEmployeeService
    {
        Task<List<Employee>> GetAllAsync();

        Task<Employee?> GetByIdAsync(string id);

        Task CreateAsync(Employee employee);

        Task UpdateAsync(Employee employee);

        Task<DeactivationOutcome> DeactivateAsync(string id);

        Task<Employee?> GetByUserIdAsync(string userId);

        Task<Employee?> GetByEmailAsync(string email);
    }
}
