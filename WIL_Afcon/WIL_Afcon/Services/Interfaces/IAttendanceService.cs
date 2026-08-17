using WIL_Afcon.Models;

namespace WIL_Afcon.Services.Interfaces
{
    public interface IAttendanceService
    {
        Task<List<Attendance>> GetAllAsync();

        Task<Attendance?> GetByIdAsync(string id);

        Task<List<Attendance>> GetByEmployeeIdAsync(string employeeId);

        Task CreateAsync(Attendance attendance);

        Task UpdateAsync(Attendance attendance);

        Task DeleteAsync(string id);
    }
}