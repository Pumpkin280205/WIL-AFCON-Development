using WIL_Afcon.API.Models;
using WIL_Afcon.API.Repositories.Interfaces;
using WIL_Afcon.API.Services.Interfaces;

namespace WIL_Afcon.API.Services
{
    public class DepartmentService : IDepartmentService
    {
        private readonly IDepartmentRepository _departmentRepository;

        public DepartmentService(IDepartmentRepository departmentRepository)
        {
            _departmentRepository = departmentRepository;
        }

        public async Task<List<Department>> GetAllAsync()
        {
            return await _departmentRepository.GetAllAsync();
        }

        public async Task<Department?> GetByIdAsync(string id)
        {
            return await _departmentRepository.GetByIdAsync(id);
        }

        public async Task CreateAsync(Department department)
        {
            await _departmentRepository.CreateAsync(department);
        }
    }
}