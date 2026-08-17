using MongoDB.Driver;
using WIL_Afcon.Data;
using WIL_Afcon.Models;
using WIL_Afcon.Repositories.Interfaces;

namespace WIL_Afcon.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly IMongoCollection<Employee> _employees;

        public EmployeeRepository(MongoDbContext context)
        {
            _employees = context.Database.GetCollection<Employee>("Employees");
        }

        public async Task<List<Employee>> GetAllAsync()
        {
            return await _employees
                .Find(_ => true)
                .ToListAsync();
        }

        public async Task<Employee?> GetByIdAsync(string id)
        {
            return await _employees
                .Find(employee => employee.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task CreateAsync(Employee employee)
        {
            await _employees.InsertOneAsync(employee);
        }

        public async Task UpdateAsync(Employee employee)
        {
            await _employees.ReplaceOneAsync(
                existing => existing.Id == employee.Id,
                employee);
        }

        public async Task DeleteAsync(string id)
        {
            await _employees.DeleteOneAsync(
                employee => employee.Id == id);
        }
    }
}