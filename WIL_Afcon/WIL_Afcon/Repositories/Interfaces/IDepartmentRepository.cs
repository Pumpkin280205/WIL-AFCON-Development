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

// Microsoft (2026) 'Task Class', Microsoft Learn.

// Available at: https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task

// (Accessed: 26 September 2026)
