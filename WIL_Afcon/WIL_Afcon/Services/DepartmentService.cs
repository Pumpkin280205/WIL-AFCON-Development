using WIL_Afcon.Models;
using WIL_Afcon.Repositories.Interfaces;
using WIL_Afcon.Services.Interfaces;

namespace WIL_Afcon.Services
{
    public class DepartmentService : IDepartmentService
    {
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IEmployeeRepository _employeeRepository;

        public DepartmentService(
            IDepartmentRepository departmentRepository,
            IEmployeeRepository employeeRepository)
        {
            _departmentRepository = departmentRepository;
            _employeeRepository = employeeRepository;
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

        public async Task<int> DeleteAsync(string id)
        {
            // shortcut: loads every employee to count them, fine at this company's size; use a count query if it grows large
            var assigned = (await _employeeRepository.GetAllAsync())
                .Count(e => e.DepartmentId == id);

            if (assigned == 0)
                await _departmentRepository.DeleteAsync(id);

            return assigned;
        }
    }
}