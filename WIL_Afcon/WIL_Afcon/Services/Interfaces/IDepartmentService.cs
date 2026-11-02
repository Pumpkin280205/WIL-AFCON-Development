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

        // Deletes the department unless employees are still assigned to it.
        // Returns how many employees are blocking the delete (0 = deleted).
        Task<int> DeleteAsync(string id);
    }
}