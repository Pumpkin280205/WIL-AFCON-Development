using WIL_Afcon.Models.ViewModels;

namespace WIL_Afcon.Services.Interfaces
{
    public interface IAttendanceReportService
    {
        /// <summary>
        /// Builds the monthly attendance register for one employee.
        /// Returns null if the employee does not exist.
        /// </summary>
        Task<AttendanceReportViewModel?> GenerateMonthlyReportAsync(
            string employeeId,
            int year,
            int month);

        /// <summary>
        /// Converts a report into CSV text (opens directly in Excel).
        /// </summary>
        string BuildCsv(AttendanceReportViewModel report);
    }
}