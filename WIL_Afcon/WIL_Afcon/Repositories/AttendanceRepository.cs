using MongoDB.Driver;
using WIL_Afcon.Data;
using WIL_Afcon.Models;
using WIL_Afcon.Repositories.Interfaces;

namespace WIL_Afcon.Repositories
{
    public class AttendanceRepository : IAttendanceRepository
    {
        private readonly IMongoCollection<Attendance> _attendance;

        public AttendanceRepository(MongoDbContext context)

        // Reference: MongoDB (2026) 'Get Started with the .NET/C# Driver',

            // MongoDB Documentation.

            // Available at: https://www.mongodb.com/docs/drivers/csharp/current/get-started/

            // (Accessed: 26 September 2026).
        {
            _attendance = context.Database
                .GetCollection<Attendance>("Attendance");
        }

        public async Task<List<Attendance>> GetAllAsync()
        {
            return await _attendance
                .Find(_ => true)
                .ToListAsync();
        }

        public async Task<Attendance?> GetByIdAsync(string id)
        {
            return await _attendance
                .Find(attendance => attendance.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Attendance>> GetByEmployeeIdAsync(string employeeId)
        {
            return await _attendance
                .Find(attendance => attendance.EmployeeId == employeeId)
                .ToListAsync();
        }

        public async Task<Attendance?> GetByEmployeeAndDateAsync(
           string employeeId,
           DateTime date)
        {
            var start = date.Date;
            var end = start.AddDays(1);

            return await _attendance
                .Find(attendance =>
                    attendance.EmployeeId == employeeId &&
                    attendance.AttendanceDate >= start &&
                    attendance.AttendanceDate < end)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Attendance>> GetByDateRangeAsync(DateTime from,DateTime to)
        {
            var start = from.Date;
            var end = to.Date.AddDays(1);

            return await _attendance
                .Find(attendance =>
                    attendance.AttendanceDate >= start &&
                    attendance.AttendanceDate < end)
                .SortByDescending(attendance => attendance.AttendanceDate)
                .ToListAsync();
        }


// Reference: MongoDB (2026) 'Insert Documents - .NET/C# Driver',

        // MongoDB Documentation.

        // Available at: https://www.mongodb.com/docs/drivers/csharp/current/crud/insert/

        // (Accessed: 26 September 2026).
        public async Task CreateAsync(Attendance attendance)
        {
            await _attendance.InsertOneAsync(attendance);
        }

// Reference: MongoDB (2026) 'Update One - .NET/C# Driver',

        // MongoDB Documentation.

        // Available at: https://www.mongodb.com/docs/drivers/csharp/current/crud/update-one/

        // (Accessed: 26 September 2026)
        public async Task UpdateAsync(Attendance attendance)
        {
            await _attendance.ReplaceOneAsync(
                existing => existing.Id == attendance.Id,
                attendance);
        }


// Reference: MongoDB (2026) 'Delete Documents - .NET/C# Driver',

        // MongoDB Documentation.

        // Available at: https://www.mongodb.com/docs/drivers/csharp/current/crud/delete/

        // (Accessed: 26 September 2026).
        public async Task DeleteAsync(string id)
        {
            await _attendance.DeleteOneAsync(
                attendance => attendance.Id == id);
        }
    }
}

// REFERENCES

// ============================================================

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
