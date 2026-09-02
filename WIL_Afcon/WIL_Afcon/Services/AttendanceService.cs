using WIL_Afcon.Models;
using WIL_Afcon.Models.Enums;
using WIL_Afcon.Models.ViewModels;
using WIL_Afcon.Repositories.Interfaces;
using WIL_Afcon.Services.Interfaces;

namespace WIL_Afcon.Services
{
    public class AttendanceService : IAttendanceService
    {
        // Clock-ins after this time of day are marked Late.
        private static readonly TimeSpan LateThreshold = new(9, 0, 0);

        private readonly IAttendanceRepository _attendanceRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IDepartmentRepository _departmentRepository;


        public AttendanceService(
           IAttendanceRepository attendanceRepository,
           IEmployeeRepository employeeRepository,
           IDepartmentRepository departmentRepository)
        {
            _attendanceRepository = attendanceRepository;
            _employeeRepository = employeeRepository;
            _departmentRepository = departmentRepository;
        }

        public async Task<List<Attendance>> GetAllAsync()
        {
            return await _attendanceRepository.GetAllAsync();
        }

        public async Task<Attendance?> GetByIdAsync(string id)
        {
            return await _attendanceRepository.GetByIdAsync(id);
        }

        public async Task<List<Attendance>> GetByEmployeeIdAsync(
            string employeeId)
        {
            return await _attendanceRepository
                .GetByEmployeeIdAsync(employeeId);
        }

        public async Task CreateAsync(Attendance attendance)
        {
            await _attendanceRepository.CreateAsync(attendance);
        }

        public async Task UpdateAsync(Attendance attendance)
        {
            await _attendanceRepository.UpdateAsync(attendance);
        }

        public async Task DeleteAsync(string id)
        {
            await _attendanceRepository.DeleteAsync(id);
        }

        public async Task<Attendance?> GetTodayForEmployeeAsync(
            string employeeId)
        {
            return await _attendanceRepository
                .GetByEmployeeAndDateAsync(employeeId, DateTime.Today);
        }

        public async Task<Attendance> ClockInAsync(string employeeId)
        {
            var record = await _attendanceRepository
                .GetByEmployeeAndDateAsync(employeeId, DateTime.UtcNow);

            record ??= new Attendance
            {
                EmployeeId = employeeId,
                AttendanceDate = DateTime.Today
            };

            if (record.ClockInTime != null)
            {
                throw new InvalidOperationException(
                    "You have already clocked in today.");
            }

            var now = DateTime.Now;

            record.ClockInTime = now;
            record.AttendanceStatus = now.TimeOfDay > LateThreshold
                ? AttendanceStatus.Late
                : AttendanceStatus.Present;

            if (string.IsNullOrEmpty(record.Id))
            {
                await _attendanceRepository.CreateAsync(record);
            }
            else
            {
                await _attendanceRepository.UpdateAsync(record);
            }

            return record;
        }

        public async Task<Attendance> ClockOutAsync(string employeeId)
        {
            var record = await GetOpenRecordAsync(employeeId);

            var now = DateTime.UtcNow;

            // Close a break that is still running.
            var openBreak = record.Breaks
                .FirstOrDefault(b => b.EndTime == null);

            if (openBreak != null)
            {
                openBreak.EndTime = now;
                openBreak.DurationMinutes =
                    ToMinutes(openBreak.StartTime, now);
            }

            record.ClockOutTime = now;

            Recalculate(record);

            await _attendanceRepository.UpdateAsync(record);

            return record;
        }

        public async Task<Attendance> StartBreakAsync(string employeeId)
        {
            var record = await GetOpenRecordAsync(employeeId);

            if (record.Breaks.Any(b => b.EndTime == null))
            {
                throw new InvalidOperationException(
                    "You are already on a break.");
            }

            record.Breaks.Add(new BreakRecord
            {
                StartTime = DateTime.UtcNow
            });

            await _attendanceRepository.UpdateAsync(record);

            return record;
        }

        public async Task<Attendance> EndBreakAsync(string employeeId)
        {
            var record = await GetOpenRecordAsync(employeeId);

            var openBreak = record.Breaks
                .FirstOrDefault(b => b.EndTime == null);

            if (openBreak == null)
            {
                throw new InvalidOperationException(
                    "You are not currently on a break.");
            }

            var now = DateTime.UtcNow;

            openBreak.EndTime = now;
            openBreak.DurationMinutes = ToMinutes(openBreak.StartTime, now);

            await _attendanceRepository.UpdateAsync(record);

            return record;
        }

        public async Task<List<AttendanceListItemViewModel>>
            GetListForAdminAsync(DateTime from, DateTime to)
        {
            var records = await _attendanceRepository
                .GetByDateRangeAsync(from, to);

            var employees = await _employeeRepository.GetAllAsync();
            var departments = await _departmentRepository.GetAllAsync();

            return records
                .Select(record =>
                {
                    var employee = employees
                        .FirstOrDefault(e => e.Id == record.EmployeeId);

                    var department = departments
                        .FirstOrDefault(d => d.Id == employee?.DepartmentId);

                    return new AttendanceListItemViewModel
                    {
                        Id = record.Id,
                        EmployeeName = employee == null
                            ? "(unknown)"
                            : $"{employee.FirstName} {employee.LastName}",
                        EmployeeNumber = employee?.EmployeeNumber ?? "-",
                        DepartmentName = department?.DepartmentName ?? "-",
                        AttendanceDate = record.AttendanceDate,
                        ClockInTime = record.ClockInTime,
                        ClockOutTime = record.ClockOutTime,
                        AttendanceStatus = record.AttendanceStatus,
                        WorkingMinutes = record.WorkingMinutes
                    };
                })
                .ToList();
        }

        private async Task<Attendance> GetOpenRecordAsync(string employeeId)
        {
            var record = await _attendanceRepository
                .GetByEmployeeAndDateAsync(employeeId, DateTime.Today);

            if (record?.ClockInTime == null)
            {
                throw new InvalidOperationException(
                    "You have not clocked in today.");
            }

            if (record.ClockOutTime != null)
            {
                throw new InvalidOperationException(
                    "You have already clocked out today.");
            }

            return record;
        }

        private static int ToMinutes(DateTime start, DateTime end)
        {
            return Math.Max(0, (int)Math.Round((end.ToUniversalTime() - start.ToUniversalTime()).TotalMinutes));
        }

        private static void Recalculate(Attendance attendance)
        {
            if (attendance.ClockInTime == null ||
                attendance.ClockOutTime == null)
            {
                attendance.WorkingMinutes = 0;
                return;
            }

            var worked = ToMinutes(
                attendance.ClockInTime.Value,
                attendance.ClockOutTime.Value);

            var breakMinutes = attendance.Breaks
                .Where(b => b.EndTime != null)
                .Sum(b => b.DurationMinutes);

            attendance.WorkingMinutes = Math.Max(0, worked - breakMinutes);
        }
    }
}