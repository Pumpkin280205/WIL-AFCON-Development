using MongoDB.Driver;
using WIL_Afcon.API.Data;
using WIL_Afcon.API.Models;
using WIL_Afcon.API.Repositories.Interfaces;

namespace WIL_Afcon.API.Repositories
{
    public class AttendanceRequestRepository : IAttendanceRequestRepository
    {
        private readonly IMongoCollection<AttendanceRequest> _attendanceRequests;

        public AttendanceRequestRepository(MongoDbContext context)
        {
            _attendanceRequests =
                context.Database.GetCollection<AttendanceRequest>("AttendanceRequests");
        }

        public async Task<List<AttendanceRequest>> GetAllAsync()
        {
            return await _attendanceRequests
                .Find(FilterDefinition<AttendanceRequest>.Empty)
                .ToListAsync();
        }

        public async Task<AttendanceRequest?> GetByIdAsync(string id)
        {
            return await _attendanceRequests
                .Find(r => r.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<List<AttendanceRequest>> GetByEmployeeIdAsync(string employeeId)
        {
            return await _attendanceRequests
                .Find(r => r.EmployeeId == employeeId)
                .ToListAsync();
        }

        public async Task<List<AttendanceRequest>> GetPendingAsync()
        {
            return await _attendanceRequests
                .Find(r => r.Status == Models.enums.AttendanceRequestStatus.Pending)
                .ToListAsync();
        }

        public async Task<AttendanceRequest> CreateAsync(AttendanceRequest request)
        {
            await _attendanceRequests.InsertOneAsync(request);

            return request;
        }


        public async Task UpdateAsync(AttendanceRequest request)
        {
            await _attendanceRequests
                .ReplaceOneAsync(r => r.Id == request.Id, request);
        }
    }
}