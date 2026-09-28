using Microsoft.Extensions.Options;
using WIL_Afcon.API.Configuration;
using WIL_Afcon.API.Models;
using WIL_Afcon.API.Models.Enums;
using WIL_Afcon.API.Repositories.Interfaces;
using WIL_Afcon.API.Services.Interfaces;

namespace WIL_Afcon.API.Services
{
    public class AttendanceService : IAttendanceService
    {
        private readonly IAttendanceRepository _attendanceRepository;
        private readonly WorkScheduleOptions _schedule;

        public AttendanceService(
            IAttendanceRepository attendanceRepository,
            IOptions<WorkScheduleOptions> schedule)
        {
            _attendanceRepository = attendanceRepository;
            _schedule = schedule.Value;
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

        // ------------------------------------------------------------------
        // SCAN SEQUENCE: decides what a scan means and applies it
        // ------------------------------------------------------------------
        public async Task<(Attendance Record, PunchAction Action)> RegisterScanAsync(
            string employeeId,
            DateTimeOffset? eventTime = null)
        {
            var scanTime = ToLocal(eventTime);
            var timeOfDay = scanTime.TimeOfDay;

            // Rule: scans only accepted inside the working-day window.
            if (timeOfDay < _schedule.EarliestClockIn || timeOfDay > _schedule.LatestScan)
            {
                throw new InvalidOperationException(
                    $"Scan at {scanTime:HH:mm} rejected: scans are only accepted between " +
                    $"{Format(_schedule.EarliestClockIn)} and {Format(_schedule.LatestScan)}.");
            }

            var record = await _attendanceRepository
                .GetByEmployeeAndDateAsync(employeeId, scanTime.Date);

            // Scan 1 of the day = clock in.
            if (record?.ClockInTime == null)
            {
                return (await ClockInAsync(employeeId, eventTime), PunchAction.ClockIn);
            }

            // Rule: nothing more after clocking out.
            if (record.ClockOutTime != null)
            {
                throw new InvalidOperationException(
                    $"Already clocked out today at {record.ClockOutTime.Value.ToLocalTime():HH:mm}. " +
                    "No more scans are accepted until tomorrow.");
            }

            // Rule: ignore accidental double scans.
            var minutesSinceLastScan =
                (scanTime.ToUniversalTime() - GetLastScanTimeUtc(record)).TotalMinutes;

            if (minutesSinceLastScan < _schedule.MinMinutesBetweenScans)
            {
                throw new InvalidOperationException(
                    $"Scan rejected: only {Math.Max(0, minutesSinceLastScan):0} minute(s) since the last scan. " +
                    $"Wait at least {_schedule.MinMinutesBetweenScans} minutes between scans.");
            }

            // Scan 3 = end break (while on a break).
            if (record.IsOnBreak)
            {
                return (await EndBreakAsync(employeeId, eventTime), PunchAction.EndBreak);
            }

            // End of the work day = clock out.
            if (timeOfDay >= _schedule.ClockOutFrom)
            {
                return (await ClockOutAsync(employeeId, eventTime), PunchAction.ClockOut);
            }

            // Scan 2 during the work day = start break.
            if (record.Breaks.Count < _schedule.MaxBreaksPerDay)
            {
                return (await StartBreakAsync(employeeId, eventTime), PunchAction.StartBreak);
            }

            // Break already used and it is too early to clock out.
            throw new InvalidOperationException(
                $"Scan at {scanTime:HH:mm} rejected: today's break has already been taken and " +
                $"clock-out is only allowed from {Format(_schedule.ClockOutFrom)}.");
        }

        // ------------------------------------------------------------------
        // INDIVIDUAL ACTIONS (each one also enforces its own rules)
        // ------------------------------------------------------------------
        public async Task<Attendance> ClockInAsync(
            string employeeId,
            DateTimeOffset? eventTime = null)
        {
            var clockInTime = ToLocal(eventTime);
            var timeOfDay = clockInTime.TimeOfDay;

            if (timeOfDay < _schedule.EarliestClockIn || timeOfDay > _schedule.LatestClockIn)
            {
                throw new InvalidOperationException(
                    $"Clock-in at {clockInTime:HH:mm} rejected: clock-in is only allowed between " +
                    $"{Format(_schedule.EarliestClockIn)} and {Format(_schedule.LatestClockIn)}.");
            }

            var attendanceDate = clockInTime.Date;

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

            record.ClockInTime = clockInTime;

            record.AttendanceStatus = timeOfDay > _schedule.LateAfter
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

        public async Task<Attendance> StartBreakAsync(
            string employeeId,
            DateTimeOffset? eventTime = null)
        {
            var startTime = ToLocal(eventTime);
            var record = await GetOpenRecordAsync(employeeId, startTime.Date);

            if (record.IsOnBreak)
            {
                throw new InvalidOperationException("This employee is already on a break.");
            }

            if (record.Breaks.Count >= _schedule.MaxBreaksPerDay)
            {
                throw new InvalidOperationException(
                    $"Only {_schedule.MaxBreaksPerDay} break(s) allowed per day.");
            }

            if (startTime.TimeOfDay >= _schedule.ClockOutFrom)
            {
                throw new InvalidOperationException(
                    $"Breaks cannot start after {Format(_schedule.ClockOutFrom)}.");
            }

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
            var endTime = ToLocal(eventTime);
            var record = await GetOpenRecordAsync(employeeId, endTime.Date);

            var openBreak = record.Breaks.FirstOrDefault(b => b.EndTime == null);

            if (openBreak == null)
            {
                throw new InvalidOperationException("This employee is not currently on a break.");
            }

            openBreak.EndTime = endTime;
            openBreak.DurationMinutes = ToMinutes(openBreak.StartTime, endTime);

            await _attendanceRepository.UpdateAsync(record);

            return record;
        }

        public async Task<Attendance> ClockOutAsync(
            string employeeId,
            DateTimeOffset? eventTime = null)
        {
            var clockOutTime = ToLocal(eventTime);

            if (clockOutTime.TimeOfDay < _schedule.ClockOutFrom)
            {
                throw new InvalidOperationException(
                    $"Clock-out at {clockOutTime:HH:mm} rejected: clock-out is only allowed from " +
                    $"{Format(_schedule.ClockOutFrom)}.");
            }

            var record = await GetOpenRecordAsync(employeeId, clockOutTime.Date);

            // Close a break that is still running.
            var openBreak = record.Breaks.FirstOrDefault(b => b.EndTime == null);
            if (openBreak != null)
            {
                openBreak.EndTime = clockOutTime;
                openBreak.DurationMinutes = ToMinutes(openBreak.StartTime, clockOutTime);
            }

            record.ClockOutTime = clockOutTime;

            Recalculate(record);

            await _attendanceRepository.UpdateAsync(record);

            return record;
        }

        public async Task<List<Attendance>> GetListForAdminAsync(DateTime from, DateTime to)
        {
            return await _attendanceRepository.GetByDateRangeAsync(from, to);
        }

        // ------------------------------------------------------------------
        // HELPERS
        // ------------------------------------------------------------------
        private async Task<Attendance> GetOpenRecordAsync(string employeeId, DateTime date)
        {
            var record = await _attendanceRepository.GetByEmployeeAndDateAsync(employeeId, date);

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

        private static DateTime ToLocal(DateTimeOffset? eventTime)
        {
            return eventTime?.LocalDateTime ?? DateTime.Now;
        }

        private static DateTime GetLastScanTimeUtc(Attendance record)
        {
            var times = new List<DateTime> { record.ClockInTime!.Value.ToUniversalTime() };

            foreach (var b in record.Breaks)
            {
                times.Add(b.StartTime.ToUniversalTime());

                if (b.EndTime.HasValue)
                {
                    times.Add(b.EndTime.Value.ToUniversalTime());
                }
            }

            return times.Max();
        }

        private static string Format(TimeSpan time)
        {
            return time.ToString(@"hh\:mm");
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