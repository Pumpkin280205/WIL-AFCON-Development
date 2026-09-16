using MongoDB.Driver;
using WIL_Afcon.API.Data;
using WIL_Afcon.API.Models;
using WIL_Afcon.API.Repositories.Interfaces;

namespace WIL_Afcon.API.Repositories
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
            return await _employees.Find(FilterDefinition<Employee>.Empty).ToListAsync();
        }

        public async Task<Employee?> GetByIdAsync(string id)
        {
            return await _employees.Find(e => e.Id == id).FirstOrDefaultAsync();
        }

        public async Task<Employee?> GetByEmailAsync(string email)
        {
            return await _employees.Find(e => e.Email == email).FirstOrDefaultAsync();
        }

        public async Task<Employee?> GetByUserIdAsync(string userId)
        {
            return await _employees.Find(e => e.UserId == userId).FirstOrDefaultAsync();
        }

        public async Task<Employee?> GetByDeviceUserIdAsync(string deviceUserId)
        {
            return await _employees.Find(e => e.DeviceUserId == deviceUserId).FirstOrDefaultAsync();
        }

        public async Task<Employee?> GetByEmployeeNumberAsync(string employeeNumber)
        {
            return await _employees
                .Find(e => e.EmployeeNumber == employeeNumber)
                .FirstOrDefaultAsync();
        }

        public async Task CreateAsync(Employee employee)
        {
            await _employees.InsertOneAsync(employee);
        }

        public async Task UpdateAsync(Employee employee)
        {
            await _employees.ReplaceOneAsync(e => e.Id == employee.Id, employee);
        }

        public async Task DeleteAsync(string id)
        {
            await _employees.DeleteOneAsync(e => e.Id == id);
        }
    }
}