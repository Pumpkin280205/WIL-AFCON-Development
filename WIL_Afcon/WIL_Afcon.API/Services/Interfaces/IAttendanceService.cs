using WIL_Afcon.API.Models;
using WIL_Afcon.API.Services;

namespace WIL_Afcon.API.Services.Interfaces
{
    public interface IAttendanceService
    {
        Task<List<Attendance>> GetAllAsync();

        Task<List<Attendance>> GetByEmployeeIdAsync(string employeeId);

        Task<Attendance?> GetTodayForEmployeeAsync(string employeeId);

        /// <summary>
        /// Works out what a biometric scan means from what the employee has already
        /// done today and the time of day, then applies it:
        /// 1st scan = clock in, 2nd = start break, 3rd = end break,
        /// scan at the end of the work day = clock out.
        /// Throws InvalidOperationException when a scan breaks a rule.
        /// </summary>
        Task<(Attendance Record, PunchAction Action)> RegisterScanAsync(
            string employeeId,
            DateTimeOffset? eventTime = null);

        Task<Attendance> ClockInAsync(string employeeId, DateTimeOffset? eventTime = null);

        Task<Attendance> ClockOutAsync(string employeeId, DateTimeOffset? eventTime = null);

        Task<Attendance> StartBreakAsync(string employeeId, DateTimeOffset? eventTime = null);

        Task<Attendance> EndBreakAsync(string employeeId, DateTimeOffset? eventTime = null);

        Task<List<Attendance>> GetListForAdminAsync(DateTime from, DateTime to);
    }
}