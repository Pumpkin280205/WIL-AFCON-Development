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

        public async Task CreateAsync(Attendance attendance)
        {
            await _attendance.InsertOneAsync(attendance);
        }

        public async Task UpdateAsync(Attendance attendance)
        {
            await _attendance.ReplaceOneAsync(
                existing => existing.Id == attendance.Id,
                attendance);
        }

        public async Task DeleteAsync(string id)
        {
            await _attendance.DeleteOneAsync(
                attendance => attendance.Id == id);
        }
    }
}