using WIL_Afcon.API.Models;

namespace WIL_Afcon.API.Services.Interfaces
{
    public interface IAttendanceService
    {
        Task<List<Attendance>> GetAllAsync();
        Task<List<Attendance>> GetByEmployeeIdAsync(string employeeId);
        Task<Attendance?> GetTodayForEmployeeAsync(string employeeId);
        Task<Attendance> ClockInAsync(string employeeId);
        Task<Attendance> ClockOutAsync(string employeeId);
        Task<Attendance> StartBreakAsync(string employeeId);
        Task<Attendance> EndBreakAsync(string employeeId);
        Task<List<Attendance>> GetListForAdminAsync(DateTime from, DateTime to);
    }
}