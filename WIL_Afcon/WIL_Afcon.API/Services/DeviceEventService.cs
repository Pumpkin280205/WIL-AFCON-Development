using Microsoft.Extensions.Options;
using WIL_Afcon.API.Configuration;
using WIL_Afcon.API.Models;
using WIL_Afcon.API.Models.Devices;
using WIL_Afcon.API.Repositories.Interfaces;
using WIL_Afcon.API.Services.Interfaces;

namespace WIL_Afcon.API.Services
{
    public class DeviceEventService : IDeviceEventService
    {
        private readonly IAttendanceService _attendance;
        private readonly IEmployeeRepository _employees;
        private readonly DeviceHealthTracker _health;
        private readonly HikvisionDeviceOptions _options;
        private readonly ILogger<DeviceEventService> _logger;

        public DeviceEventService(
            IAttendanceService attendance,
            IEmployeeRepository employees,
            DeviceHealthTracker health,
            IOptions<HikvisionDeviceOptions> options,
            ILogger<DeviceEventService> logger)
        {
            _attendance = attendance;
            _employees = employees;
            _health = health;
            _options = options.Value;
            _logger = logger;
        }

        public async Task<string> HandleHikvisionEventAsync(AccessControllerEvent? deviceEvent)
        {
            _health.MarkEventReceived();

            var deviceUserId = deviceEvent?.EmployeeNoString?.Trim();
            if (string.IsNullOrEmpty(deviceUserId))
            {
                // Keep-alive ping, door/tamper event, or a failed authentication -
                // nothing to record, but the terminal is clearly alive.
                return "ignored: keep-alive or non-authentication event (no person attached)";
            }

            var window = TimeSpan.FromSeconds(Math.Max(1, _options.DedupeWindowSeconds));
            var dedupeKey = deviceEvent!.SerialNo is { } serial
                ? $"serial:{serial}"
                : $"{deviceUserId}:{deviceEvent.AttendanceStatus}";

            if (_health.IsDuplicate(dedupeKey, window))
            {
                return $"ignored: duplicate within {window.TotalSeconds:0}s ({dedupeKey})";
            }

            var employee = await _employees.GetByDeviceUserIdAsync(deviceUserId);
            if (employee is null)
            {
                _logger.LogWarning(
                    "Hikvision punch for unbound device user '{DeviceUserId}' ({Name}). " +
                    "Bind it with PUT /api/employees/{{id}}/device-binding.",
                    deviceUserId, deviceEvent.Name);
                return $"ignored: no employee bound to device user '{deviceUserId}'";
            }

            var action = MapAction(deviceEvent.AttendanceStatus);

            try
            {
                var updated = action switch
                {
                    PunchAction.ClockIn => await _attendance.ClockInAsync(employee.Id),
                    PunchAction.ClockOut => await _attendance.ClockOutAsync(employee.Id),
                    PunchAction.StartBreak => await _attendance.StartBreakAsync(employee.Id),
                    PunchAction.EndBreak => await _attendance.EndBreakAsync(employee.Id),
                    _ => await InferAndApplyAsync(employee.Id),
                };

                _health.MarkPunchApplied();

                return $"applied {action} for {employee.FirstName} {employee.LastName} " +
                       $"({employee.Id}); status now {updated.AttendanceStatus}";
            }
            catch (InvalidOperationException ex)
            {
                // e.g. "already clocked in today" - expected, not a server fault.
                return $"rejected for {employee.FirstName} {employee.LastName} " +
                       $"({employee.Id}): {ex.Message}";
            }
        }

        public DeviceHealthResponse GetHealth()
        {
            var last = _health.LastEventUtc;

            return new DeviceHealthResponse
            {
                ExpectedHost = string.IsNullOrWhiteSpace(_options.Host) ? null : _options.Host,
                LastEventUtc = last,
                LastPunchAppliedUtc = _health.LastPunchAppliedUtc,
                SecondsSinceLastEvent = last is { } t
                    ? (int)Math.Max(0, (DateTime.UtcNow - t).TotalSeconds)
                    : null,
                TotalEventsReceived = _health.TotalEventsReceived,
                TotalPunchesApplied = _health.TotalPunchesApplied,
            };
        }

        private static PunchAction MapAction(string? attendanceStatus)
            => attendanceStatus?.Trim().ToLowerInvariant() switch
            {
                "checkin" => PunchAction.ClockIn,
                "checkout" => PunchAction.ClockOut,
                "breakout" => PunchAction.StartBreak,
                "breakin" => PunchAction.EndBreak,
                _ => PunchAction.Infer,
            };

        /// <summary>
        /// Used when the terminal's Attendance Status feature is disabled and every
        /// scan arrives with an empty attendanceStatus: derive the next sensible step
        /// from today's record.
        /// </summary>
        private async Task<Attendance> InferAndApplyAsync(string employeeId)
        {
            var today = await _attendance.GetTodayForEmployeeAsync(employeeId);

            if (today?.ClockInTime is null)
            {
                return await _attendance.ClockInAsync(employeeId);
            }

            if (today.IsOnBreak)
            {
                return await _attendance.EndBreakAsync(employeeId);
            }

            if (today.ClockOutTime is null)
            {
                return await _attendance.ClockOutAsync(employeeId);
            }

            throw new InvalidOperationException(
                "This employee has already clocked in and out today.");
        }
    }
}
