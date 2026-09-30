using Microsoft.Extensions.Options;
using WIL_Afcon.API.Configuration;
using WIL_Afcon.API.Models;
using WIL_Afcon.API.Models.Enums;
using WIL_Afcon.API.Repositories.Interfaces;
using WIL_Afcon.API.Services;
using Xunit;

namespace WIL_Afcon.Tests
{
    // Kiaan's attendance rules (WIL_Afcon.API -> Services/AttendanceService.cs)
    //  - clock-in only allowed 06:00 - 12:00
    //  - max 1 break per day
    //  - scans less than 5 minutes apart are rejected (double scan)
    public class AttendanceRulesTests
    {
        private const string EmployeeId = "emp-1";

        // ---------- helpers ----------

        // Builds a scan time on a fixed test day at the given local hour/minute.
        private static DateTimeOffset At(int hour, int minute)
        {
            return new DateTimeOffset(
                new DateTime(2026, 3, 2, hour, minute, 0, DateTimeKind.Local));
        }

        // A fresh service with an empty in-memory "database" and the default schedule.
        private static AttendanceService CreateService(out FakeAttendanceRepository repo)
        {
            repo = new FakeAttendanceRepository();
            return new AttendanceService(repo, Options.Create(new WorkScheduleOptions()));
        }

        // ---------- clock-in window: 06:00 - 12:00 ----------

        [Theory]
        [InlineData(6, 0)]
        [InlineData(8, 30)]
        [InlineData(12, 0)]
        public async Task ClockIn_InsideWindow_IsAccepted(int hour, int minute)
        {
            var service = CreateService(out var repo);

            var record = await service.ClockInAsync(EmployeeId, At(hour, minute));

            Assert.NotNull(record.ClockInTime);
            Assert.Single(repo.Records);
        }

        [Theory]
        [InlineData(5, 59)]
        [InlineData(12, 1)]
        [InlineData(14, 0)]
        public async Task ClockIn_OutsideWindow_IsRejected(int hour, int minute)
        {
            var service = CreateService(out var repo);

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.ClockInAsync(EmployeeId, At(hour, minute)));

            Assert.Empty(repo.Records);
        }

        [Fact]
        public async Task ClockIn_Before0900_IsPresent_After0900_IsLate()
        {
            var onTimeService = CreateService(out _);
            var lateService = CreateService(out _);

            var onTime = await onTimeService.ClockInAsync(EmployeeId, At(8, 45));
            var late = await lateService.ClockInAsync(EmployeeId, At(9, 30));

            Assert.Equal(AttendanceStatus.Present, onTime.AttendanceStatus);
            Assert.Equal(AttendanceStatus.Late, late.AttendanceStatus);
        }

        [Fact]
        public async Task ClockIn_Twice_SameDay_IsRejected()
        {
            var service = CreateService(out _);
            await service.ClockInAsync(EmployeeId, At(8, 0));

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.ClockInAsync(EmployeeId, At(9, 0)));
        }

        // ---------- breaks capped at 1 per day ----------

        [Fact]
        public async Task StartBreak_SecondBreakSameDay_IsRejected()
        {
            var service = CreateService(out _);
            await service.ClockInAsync(EmployeeId, At(8, 0));
            await service.StartBreakAsync(EmployeeId, At(10, 0));
            await service.EndBreakAsync(EmployeeId, At(10, 30));

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.StartBreakAsync(EmployeeId, At(11, 0)));
        }

        [Fact]
        public async Task Scan_AfterBreakAlreadyUsed_BeforeClockOutTime_IsRejected()
        {
            var service = CreateService(out _);
            await service.RegisterScanAsync(EmployeeId, At(8, 0));   // clock in
            await service.RegisterScanAsync(EmployeeId, At(10, 0));  // start break
            await service.RegisterScanAsync(EmployeeId, At(10, 30)); // end break

            // A 4th scan at 11:00 would be a 2nd break - not allowed.
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.RegisterScanAsync(EmployeeId, At(11, 0)));
        }

        [Fact]
        public async Task Scan_Sequence_ClockIn_Break_EndBreak_ClockOut()
        {
            var service = CreateService(out _);

            var first = await service.RegisterScanAsync(EmployeeId, At(8, 0));
            var second = await service.RegisterScanAsync(EmployeeId, At(12, 0));
            var third = await service.RegisterScanAsync(EmployeeId, At(12, 30));
            var fourth = await service.RegisterScanAsync(EmployeeId, At(16, 0));

            Assert.Equal(PunchAction.ClockIn, first.Action);
            Assert.Equal(PunchAction.StartBreak, second.Action);
            Assert.Equal(PunchAction.EndBreak, third.Action);
            Assert.Equal(PunchAction.ClockOut, fourth.Action);

            // 8h on site minus a 30 min break = 450 working minutes.
            Assert.Equal(450, fourth.Record.WorkingMinutes);
        }

        // ---------- double scans within 5 minutes ----------

        [Fact]
        public async Task Scan_Within5Minutes_OfClockIn_IsRejected()
        {
            var service = CreateService(out var repo);
            await service.RegisterScanAsync(EmployeeId, At(8, 0));

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.RegisterScanAsync(EmployeeId, At(8, 3)));

            // The rejected scan must not have started a break.
            Assert.Empty(repo.Records.Single().Breaks);
        }

        [Fact]
        public async Task Scan_Exactly5MinutesLater_IsAccepted()
        {
            var service = CreateService(out _);
            await service.RegisterScanAsync(EmployeeId, At(8, 0));

            var result = await service.RegisterScanAsync(EmployeeId, At(8, 5));

            Assert.Equal(PunchAction.StartBreak, result.Action);
        }

        [Fact]
        public async Task Scan_Within5Minutes_OfEndingBreak_IsRejected()
        {
            var service = CreateService(out _);
            await service.RegisterScanAsync(EmployeeId, At(8, 0));   // clock in
            await service.RegisterScanAsync(EmployeeId, At(10, 0));  // start break
            await service.RegisterScanAsync(EmployeeId, At(10, 30)); // end break

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.RegisterScanAsync(EmployeeId, At(10, 32)));
        }

        // ---------- fake repository (in memory, no MongoDB) ----------

        public class FakeAttendanceRepository : IAttendanceRepository
        {
            public List<Attendance> Records { get; } = new();

            public Task<Attendance?> GetByEmployeeAndDateAsync(string employeeId, DateTime date) =>
                Task.FromResult(Records.FirstOrDefault(
                    a => a.EmployeeId == employeeId && a.AttendanceDate.Date == date.Date));

            public Task CreateAsync(Attendance attendance)
            {
                attendance.Id = Guid.NewGuid().ToString();
                Records.Add(attendance);
                return Task.CompletedTask;
            }

            public Task UpdateAsync(Attendance attendance) => Task.CompletedTask;

            public Task<List<Attendance>> GetAllAsync() => Task.FromResult(Records.ToList());

            public Task<Attendance?> GetByIdAsync(string id) =>
                Task.FromResult(Records.FirstOrDefault(a => a.Id == id));

            public Task<List<Attendance>> GetByEmployeeIdAsync(string employeeId) =>
                Task.FromResult(Records.Where(a => a.EmployeeId == employeeId).ToList());

            public Task<List<Attendance>> GetByDateRangeAsync(DateTime from, DateTime to) =>
                Task.FromResult(Records.Where(a => a.AttendanceDate >= from && a.AttendanceDate <= to).ToList());

            public Task DeleteAsync(string id)
            {
                Records.RemoveAll(a => a.Id == id);
                return Task.CompletedTask;
            }
        }
    }
}