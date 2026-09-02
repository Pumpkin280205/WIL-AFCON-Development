using WIL_Afcon.Models;
using WIL_Afcon.Models.ViewModels;

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

        Task<Attendance?> GetTodayForEmployeeAsync(string employeeId);

        Task<Attendance> ClockInAsync(string employeeId);

        Task<Attendance> ClockOutAsync(string employeeId);

        Task<Attendance> StartBreakAsync(string employeeId);

        Task<Attendance> EndBreakAsync(string employeeId);

        Task<List<AttendanceListItemViewModel>> GetListForAdminAsync(
            DateTime from,
            DateTime to);
    }
}