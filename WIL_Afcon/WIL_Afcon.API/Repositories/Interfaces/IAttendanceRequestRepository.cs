using WIL_Afcon.API.Models;

namespace WIL_Afcon.API.Repositories.Interfaces
{
     public interface IAttendanceRequestRepository
    {
        Task<AttendanceRequest> CreateAsync(AttendanceRequest request);

        Task<AttendanceRequest?> GetByIdAsync(string id);

        Task<List<AttendanceRequest>> GetAllAsync();

        Task<List<AttendanceRequest>> GetByEmployeeIdAsync(string employeeId);

        Task<List<AttendanceRequest>> GetPendingAsync();

        Task UpdateAsync(AttendanceRequest request);

        Task DeleteAsync(string id);
    }
}