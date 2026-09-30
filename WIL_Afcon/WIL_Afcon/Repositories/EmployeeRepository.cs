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

        public async Task<Employee?> GetByEmailAsync(string email)
        {
            return await _employees
                .Find(employee => employee.Email == email)
                .FirstOrDefaultAsync();
        }

   public async Task<Employee?> GetByUserIdAsync(string userId)
        {
            return await _employees
                .Find(employee => employee.UserId == userId)
                .FirstOrDefaultAsync();
        }

        public async Task<Employee?> GetByDeviceUserIdAsync(string deviceUserId)
        {
            return await _employees
                .Find(employee => employee.DeviceUserId == deviceUserId)
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

// MongoDB (2026) 'Delete Documents - .NET/C# Driver',

// MongoDB Documentation.

// Available at: https://www.mongodb.com/docs/drivers/csharp/current/crud/delete/

// (Accessed: 26 September 2026).

//

// MongoDB (2026) 'Get Started with the .NET/C# Driver',

// MongoDB Documentation.

// Available at: https://www.mongodb.com/docs/drivers/csharp/current/get-started/

// (Accessed: 26 September 2026).

//

// MongoDB (2026) 'Insert Documents - .NET/C# Driver',

// MongoDB Documentation.

// Available at: https://www.mongodb.com/docs/drivers/csharp/current/crud/insert/

// (Accessed: 26 September 2026).

//

// MongoDB (2026) 'Retrieve Data - .NET/C# Driver',

// MongoDB Documentation.

// Available at: https://www.mongodb.com/docs/drivers/csharp/current/crud/query/find/

// (Accessed: 26 September 2026).

//

// MongoDB (2026) 'Update One - .NET/C# Driver',

// MongoDB Documentation.

// Available at: https://www.mongodb.com/docs/drivers/csharp/current/crud/update-one/

// (Accessed: 26 September 2026)
