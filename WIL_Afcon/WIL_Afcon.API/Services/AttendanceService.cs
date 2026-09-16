using WIL_Afcon.API.Models;
using WIL_Afcon.API.Models.Enums;
using WIL_Afcon.API.Repositories.Interfaces;
using WIL_Afcon.API.Services.Interfaces;

namespace WIL_Afcon.API.Services
{
    public class AttendanceService : IAttendanceService
    {
        private static readonly TimeSpan LateThreshold = new(9, 0, 0);

        private readonly IAttendanceRepository _attendanceRepository;

        public AttendanceService(IAttendanceRepository attendanceRepository)
        {
            _attendanceRepository = attendanceRepository;
        }

        public async Task<List<Attendance>> GetAllAsync()
        {
            return await _attendanceRepository.GetAllAsync();
        }

        public async Task<List<Attendance>> GetByEmployeeIdAsync(string employeeId)
        {
            return await _attendanceRepository.GetByEmployeeIdAsync(employeeId);
        }

        public async Task<Attendance?> GetTodayForEmployeeAsync(string employeeId)
        {
            return await _attendanceRepository.GetByEmployeeAndDateAsync(employeeId, DateTime.Today);
        }

        public async Task<Attendance> ClockInAsync(
     string employeeId,
     DateTimeOffset? eventTime = null)
        {
            var effectiveTime = eventTime?.LocalDateTime ?? DateTime.Now;
            var attendanceDate = effectiveTime.Date;

            var record = await _attendanceRepository
                .GetByEmployeeAndDateAsync(employeeId, attendanceDate);

            record ??= new Attendance
            {
                EmployeeId = employeeId,
                AttendanceDate = attendanceDate
            };

            if (record.ClockInTime != null)
            {
                throw new InvalidOperationException(
                    "This employee has already clocked in today.");
            }

            record.ClockInTime = effectiveTime;

            record.AttendanceStatus = effectiveTime.TimeOfDay > LateThreshold
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

        public async Task<Attendance> ClockOutAsync(
      string employeeId,
      DateTimeOffset? eventTime = null)
        {
            var record = await GetOpenRecordAsync(employeeId);

            var now = eventTime?.LocalDateTime ?? DateTime.Now;

            var openBreak = record.Breaks.FirstOrDefault(b => b.EndTime == null);
            if (openBreak != null)
            {
                openBreak.EndTime = now;
                openBreak.DurationMinutes = ToMinutes(openBreak.StartTime, now);
            }

            record.ClockOutTime = now;

            Recalculate(record);

            await _attendanceRepository.UpdateAsync(record);

            return record;
        }

        public async Task<Attendance> StartBreakAsync(
     string employeeId,
     DateTimeOffset? eventTime = null)
        {
            var record = await GetOpenRecordAsync(employeeId);

            if (record.Breaks.Any(b => b.EndTime == null))
            {
                throw new InvalidOperationException("This employee is already on a break.");
            }

            var startTime = eventTime?.LocalDateTime ?? DateTime.Now;

            record.Breaks.Add(new BreakRecord
            {
                StartTime = startTime
            });

            await _attendanceRepository.UpdateAsync(record);

            return record;
        }

        public async Task<Attendance> EndBreakAsync(
    string employeeId,
    DateTimeOffset? eventTime = null)
        {
            var record = await GetOpenRecordAsync(employeeId);

            var openBreak = record.Breaks.FirstOrDefault(b => b.EndTime == null);
            if (openBreak == null)
            {
                throw new InvalidOperationException("This employee is not currently on a break.");
            }

            var now = eventTime?.LocalDateTime ?? DateTime.Now;
            openBreak.EndTime = now;
            openBreak.DurationMinutes = ToMinutes(openBreak.StartTime, now);

            await _attendanceRepository.UpdateAsync(record);

            return record;
        }

        public async Task<List<Attendance>> GetListForAdminAsync(DateTime from, DateTime to)
        {
            return await _attendanceRepository.GetByDateRangeAsync(from, to);
        }

        private async Task<Attendance> GetOpenRecordAsync(string employeeId)
        {
            var record = await _attendanceRepository.GetByEmployeeAndDateAsync(employeeId, DateTime.Today);

            if (record?.ClockInTime == null)
            {
                throw new InvalidOperationException("This employee has not clocked in today.");
            }

            if (record.ClockOutTime != null)
            {
                throw new InvalidOperationException("This employee has already clocked out today.");
            }

            return record;
        }

        private static int ToMinutes(DateTime start, DateTime end)
        {
            return Math.Max(0, (int)Math.Round((end.ToUniversalTime() - start.ToUniversalTime()).TotalMinutes));
        }

        private static void Recalculate(Attendance attendance)
        {
            if (attendance.ClockInTime == null || attendance.ClockOutTime == null)
            {
                attendance.WorkingMinutes = 0;
                return;
            }

            var worked = ToMinutes(attendance.ClockInTime.Value, attendance.ClockOutTime.Value);

            var breakMinutes = attendance.Breaks
                .Where(b => b.EndTime != null)
                .Sum(b => b.DurationMinutes);

            attendance.WorkingMinutes = Math.Max(0, worked - breakMinutes);
        }
    }
}