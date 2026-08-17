using WIL_Afcon.Models;
using WIL_Afcon.Repositories.Interfaces;
using WIL_Afcon.Services.Interfaces;

namespace WIL_Afcon.Services
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

        public async Task UpdateAsync(Department department)
        {
            await _departmentRepository.UpdateAsync(department);
        }

        public async Task DeactivateAsync(string id)
        {
            var department = await _departmentRepository.GetByIdAsync(id);

            if (department == null)
                return;

            department.IsActive = false;

            await _departmentRepository.UpdateAsync(department);
        }
    }
}