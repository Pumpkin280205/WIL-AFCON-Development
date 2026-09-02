using WIL_Afcon.Models;
namespace WIL_Afcon.Repositories.Interfaces
{
    public interface IAttendanceRepository
    {
        Task<List<Attendance>> GetAllAsync();

        Task<Attendance?> GetByIdAsync(string id);

        Task<List<Attendance>> GetByEmployeeIdAsync(string employeeId);

        Task<Attendance?> GetByEmployeeAndDateAsync(
           string employeeId,
           DateTime date);

        Task<List<Attendance>> GetByDateRangeAsync(DateTime from, DateTime to);

        Task CreateAsync(Attendance attendance);

        Task UpdateAsync(Attendance attendance);

        Task DeleteAsync(string id);
    }
}
