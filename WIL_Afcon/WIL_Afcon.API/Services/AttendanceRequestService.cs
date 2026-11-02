using WIL_Afcon.API.Models;
using WIL_Afcon.API.Repositories.Interfaces;
using WIL_Afcon.API.Services.Interfaces;

namespace WIL_Afcon.API.Services
{
    public class AttendanceRequestService : IAttendanceRequestService
    {
        private readonly IAttendanceRequestRepository _attendanceRequests;

        public AttendanceRequestService(
            IAttendanceRequestRepository attendanceRequests)
        {
            _attendanceRequests = attendanceRequests;
        }

        public async Task<AttendanceRequest> CreateAsync(
            AttendanceRequest request)
        {
            request.Status = Models.enums.AttendanceRequestStatus.Pending;
            request.CreatedAt = DateTime.UtcNow;

            return await _attendanceRequests.CreateAsync(request);
        }

        public async Task<AttendanceRequest?> GetByIdAsync(string id)
        {
            return await _attendanceRequests.GetByIdAsync(id);
        }

        public async Task<List<AttendanceRequest>> GetAllAsync()
        {
            return await _attendanceRequests.GetAllAsync();
        }

        public async Task<List<AttendanceRequest>> GetByEmployeeIdAsync(
            string employeeId)
        {
            return await _attendanceRequests
                .GetByEmployeeIdAsync(employeeId);
        }

        public async Task<List<AttendanceRequest>> GetPendingAsync()
        {
            return await _attendanceRequests.GetPendingAsync();
        }

        public async Task<bool> AcceptAsync(
            string id,
            string adminComment,
            string reviewedBy)
        {
            var request = await _attendanceRequests.GetByIdAsync(id);

            if (request == null)
            {
                return false;
            }

            request.Status =
                Models.enums.AttendanceRequestStatus.Accepted;

            request.AdminComment = adminComment;
            request.ReviewedBy = reviewedBy;
            request.ReviewedAt = DateTime.UtcNow;

            await _attendanceRequests.UpdateAsync(request);

            return true;
        }

        public async Task<bool> RejectAsync(
            string id,
            string adminComment,
            string reviewedBy)
        {
            var request = await _attendanceRequests.GetByIdAsync(id);

            if (request == null)
            {
                return false;
            }

            request.Status =
                Models.enums.AttendanceRequestStatus.Rejected;

            request.AdminComment = adminComment;
            request.ReviewedBy = reviewedBy;
            request.ReviewedAt = DateTime.UtcNow;

            await _attendanceRequests.UpdateAsync(request);

            return true;
        }

        public async Task DeleteAsync(string id)
        {
            await _attendanceRequests.DeleteAsync(id);
        }
    }
}