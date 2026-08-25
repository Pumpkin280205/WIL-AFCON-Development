using WIL_Afcon.Models;

namespace WIL_Afcon.Services.Interfaces
{
    public interface IEmployeeService
    {
        Task<List<Employee>> GetAllAsync();

        Task<Employee?> GetByIdAsync(string id);

        Task CreateAsync(Employee employee);

        Task UpdateAsync(Employee employee);

        Task DeactivateAsync(string id);

        Task<Employee?> GetByUserIdAsync(string userId);
    }
}
