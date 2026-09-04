using MongoDB.Driver;
using WIL_Afcon.API.Data;
using WIL_Afcon.API.Models;
using WIL_Afcon.API.Repositories.Interfaces;

namespace WIL_Afcon.API.Repositories
{
    public class AttendanceRepository : IAttendanceRepository
    {
        private readonly IMongoCollection<Attendance> _attendance;

        public AttendanceRepository(MongoDbContext context)
        {
            _attendance = context.Database.GetCollection<Attendance>("Attendance");
        }

        public async Task<List<Attendance>> GetAllAsync()
        {
            return await _attendance.Find(FilterDefinition<Attendance>.Empty).ToListAsync();
        }

        public async Task<Attendance?> GetByIdAsync(string id)
        {
            return await _attendance.Find(a => a.Id == id).FirstOrDefaultAsync();
        }

        public async Task<List<Attendance>> GetByEmployeeIdAsync(string employeeId)
        {
            return await _attendance.Find(a => a.EmployeeId == employeeId).ToListAsync();
        }

        public async Task<Attendance?> GetByEmployeeAndDateAsync(string employeeId, DateTime date)
        {
            var start = date.Date;
            var end = start.AddDays(1);

            return await _attendance
                .Find(a => a.EmployeeId == employeeId &&
                           a.AttendanceDate >= start &&
                           a.AttendanceDate < end)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Attendance>> GetByDateRangeAsync(DateTime from, DateTime to)
        {
            var start = from.Date;
            var end = to.Date.AddDays(1);

            return await _attendance
                .Find(a => a.AttendanceDate >= start && a.AttendanceDate < end)
                .SortByDescending(a => a.AttendanceDate)
                .ToListAsync();
        }

        public async Task CreateAsync(Attendance attendance)
        {
            await _attendance.InsertOneAsync(attendance);
        }

        public async Task UpdateAsync(Attendance attendance)
        {
            await _attendance.ReplaceOneAsync(a => a.Id == attendance.Id, attendance);
        }

        public async Task DeleteAsync(string id)
        {
            await _attendance.DeleteOneAsync(a => a.Id == id);
        }
    }
}