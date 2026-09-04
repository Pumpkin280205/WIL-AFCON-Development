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
    }
}