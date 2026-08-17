using WIL_Afcon.Models;
namespace WIL_Afcon.Repositories.Interfaces
{
    public interface IDepartmentRepository
    {
        Task<List<Department>> GetAllAsync();

        Task<Department?> GetByIdAsync(string id);

        Task CreateAsync(Department department);

        Task UpdateAsync(Department department);

        Task DeleteAsync(string id);
    }
}
