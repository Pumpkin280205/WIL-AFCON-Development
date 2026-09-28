using WIL_Afcon.API.Models;

namespace WIL_Afcon.API.Services.Interfaces
{
    public interface IDepartmentService
    {
        Task<List<Department>> GetAllAsync();
        Task<Department?> GetByIdAsync(string id);
        Task CreateAsync(Department department);
    }
}