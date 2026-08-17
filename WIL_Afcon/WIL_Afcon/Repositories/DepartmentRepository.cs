using MongoDB.Driver;
using WIL_Afcon.Data;
using WIL_Afcon.Models;
using WIL_Afcon.Repositories.Interfaces;

namespace WIL_Afcon.Repositories
{
    public class DepartmentRepository : IDepartmentRepository
    {
        private readonly IMongoCollection<Department> _departments;

        public DepartmentRepository(MongoDbContext context)
        {
            _departments = context.Database
                .GetCollection<Department>("Departments");
        }

        public async Task<List<Department>> GetAllAsync()
        {
            return await _departments
                .Find(_ => true)
                .ToListAsync();
        }

        public async Task<Department?> GetByIdAsync(string id)
        {
            return await _departments
                .Find(department => department.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task CreateAsync(Department department)
        {
            await _departments.InsertOneAsync(department);
        }

        public async Task UpdateAsync(Department department)
        {
            await _departments.ReplaceOneAsync(
                existing => existing.Id == department.Id,
                department);
        }

        public async Task DeleteAsync(string id)
        {
            await _departments.DeleteOneAsync(
                department => department.Id == id);
        }
    }
}