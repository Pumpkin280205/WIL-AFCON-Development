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

//

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
