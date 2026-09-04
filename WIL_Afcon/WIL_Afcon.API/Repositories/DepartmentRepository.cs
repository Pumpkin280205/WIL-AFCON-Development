using MongoDB.Driver;
using WIL_Afcon.API.Data;
using WIL_Afcon.API.Models;
using WIL_Afcon.API.Repositories.Interfaces;

namespace WIL_Afcon.API.Repositories
{
    public class DepartmentRepository : IDepartmentRepository
    {
        private readonly IMongoCollection<Department> _departments;

        public DepartmentRepository(MongoDbContext context)
        {
            _departments = context.Database.GetCollection<Department>("Departments");
        }

        public async Task<List<Department>> GetAllAsync()
        {
            return await _departments.Find(FilterDefinition<Department>.Empty).ToListAsync();
        }

        public async Task<Department?> GetByIdAsync(string id)
        {
            return await _departments.Find(d => d.Id == id).FirstOrDefaultAsync();
        }

        public async Task CreateAsync(Department department)
        {
            await _departments.InsertOneAsync(department);
        }

        public async Task UpdateAsync(Department department)
        {
            await _departments.ReplaceOneAsync(d => d.Id == department.Id, department);
        }

        public async Task DeleteAsync(string id)
        {
            await _departments.DeleteOneAsync(d => d.Id == id);
        }
    }
}