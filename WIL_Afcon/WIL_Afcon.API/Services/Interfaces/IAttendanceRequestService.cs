using WIL_Afcon.API.Models;

namespace WIL_Afcon.API.Services.Interfaces
{
    public interface IAttendanceRequestService
    {
        Task<AttendanceRequest> CreateAsync(AttendanceRequest request);

        Task<AttendanceRequest?> GetByIdAsync(string id);

        Task<List<AttendanceRequest>> GetAllAsync();

        Task<List<AttendanceRequest>> GetByEmployeeIdAsync(string employeeId);

        Task<List<AttendanceRequest>> GetPendingAsync();

        Task<bool> AcceptAsync(
            string id,
            string adminComment,
            string reviewedBy);

        Task<bool> RejectAsync(
            string id,
            string adminComment,
            string reviewedBy);
    }
}