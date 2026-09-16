using WIL_Afcon.API.Models;

namespace WIL_Afcon.API.Services.Interfaces
{
    public interface IAttendanceService
    {
        Task<List<Attendance>> GetAllAsync();
        Task<List<Attendance>> GetByEmployeeIdAsync(string employeeId);
        Task<Attendance?> GetTodayForEmployeeAsync(string employeeId);
        Task<Attendance> ClockInAsync(string employeeId, DateTimeOffset? eventTime = null);
        Task<Attendance> ClockOutAsync(string employeeId, DateTimeOffset? eventTime = null);
        Task<Attendance> StartBreakAsync(string employeeId, DateTimeOffset? eventTime = null);
        Task<Attendance> EndBreakAsync(string employeeId, DateTimeOffset? eventTime = null);
        Task<List<Attendance>> GetListForAdminAsync(DateTime from, DateTime to);
    }
}