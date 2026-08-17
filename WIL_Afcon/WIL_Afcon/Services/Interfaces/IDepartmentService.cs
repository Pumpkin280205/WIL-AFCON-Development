using WIL_Afcon.Models;

namespace WIL_Afcon.Services.Interfaces
{
    public interface IDepartmentService
    {
        Task<List<Department>> GetAllAsync();

        Task<Department?> GetByIdAsync(string id);

        Task CreateAsync(Department department);

        Task UpdateAsync(Department department);

        Task DeactivateAsync(string id);
    }
}