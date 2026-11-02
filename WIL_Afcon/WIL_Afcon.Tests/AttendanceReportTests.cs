using WIL_Afcon.Models;
using WIL_Afcon.Models.Enums;
using WIL_Afcon.Models.ViewModels;
using WIL_Afcon.Repositories.Interfaces;
using WIL_Afcon.Services;
using Xunit;

namespace WIL_Afcon.Tests
{
    // Monthly report filtering / calculation logic
    // (WIL_Afcon web app -> Services/AttendanceReportService.cs)
    //
    // Test month: September 2025
    //   Mon 1 Sep  - worked 10h        -> 2h overtime
    //   Tue 2 Sep  - worked 8h (Late)  -> no overtime
    //   Wed 3 Sep  - no record         -> "Absent"   (another employee DID work - must be ignored)
    //   Sat 6 Sep  - no record         -> "Weekend"
    //   Sun 7 Sep  - worked 5h         -> Sunday hours, not overtime
    //   Wed 24 Sep - worked 6h         -> Heritage Day (public holiday) hours
    public class AttendanceReportTests
    {
        private const string EmployeeId = "emp-1";
        private const string OtherEmployeeId = "emp-2";

        private static Attendance Worked(string employeeId, int day, double hours,
            AttendanceStatus status = AttendanceStatus.Present)
        {
            var clockIn = new DateTime(2025, 9, day, 7, 0, 0);
            return new Attendance
            {
                Id = Guid.NewGuid().ToString(),
                EmployeeId = employeeId,
                AttendanceDate = new DateTime(2025, 9, day),
                ClockInTime = clockIn,
                ClockOutTime = clockIn.AddHours(hours),
                WorkingMinutes = (int)(hours * 60),
                AttendanceStatus = status
            };
        }

        private static AttendanceReportService CreateService()
        {
            var attendance = new FakeAttendanceRepository(
                Worked(EmployeeId, 1, 10),
                Worked(EmployeeId, 2, 8, AttendanceStatus.Late),
                Worked(OtherEmployeeId, 3, 9),            // someone else - must be filtered out
                Worked(EmployeeId, 7, 5),
                Worked(EmployeeId, 24, 6));

            var employees = new FakeEmployeeRepository(new Employee
            {
                Id = EmployeeId,
                FirstName = "Jane",
                LastName = "Doe",
                EmployeeNumber = "EMP001",
                DepartmentId = "dep-1"
            });

            var departments = new FakeDepartmentRepository(new Department
            {
                Id = "dep-1",
                DepartmentName = "Workshop"
            });

            return new AttendanceReportService(attendance, employees, departments);
        }

        private static AttendanceReportDayViewModel Day(AttendanceReportViewModel report, int day) =>
            report.Days.Single(d => d.DayNumber == day);

        [Fact]
        public async Task Report_HasOneRowPerDayOfMonth()
        {
            var report = await CreateService().GenerateMonthlyReportAsync(EmployeeId, 2025, 9);

            Assert.NotNull(report);
            Assert.Equal(30, report!.Days.Count);
            Assert.Equal("Jane Doe", report.EmployeeName);
            Assert.Equal("Workshop", report.DepartmentName);
        }

        [Fact]
        public async Task Report_OnlyIncludesTheSelectedEmployeesRecords()
        {
            var report = await CreateService().GenerateMonthlyReportAsync(EmployeeId, 2025, 9);

            // emp-2 worked on the 3rd, but that must not show up on emp-1's report.
            Assert.False(Day(report!, 3).HasRecord);
            Assert.Equal("Absent", Day(report!, 3).Remarks);
            Assert.Equal(4, report!.DaysPresent);
            Assert.Equal(1, report.DaysLate);
        }

        [Fact]
        public async Task Report_HoursAbove8OnAWeekday_AreOvertime()
        {
            var report = await CreateService().GenerateMonthlyReportAsync(EmployeeId, 2025, 9);

            Assert.Equal(10, Day(report!, 1).HoursWorked);
            Assert.Equal(2, Day(report!, 1).OvertimeHours);
            Assert.Equal(0, Day(report!, 2).OvertimeHours);
            Assert.Equal(2, report!.TotalOvertimeHours);
        }

        [Fact]
        public async Task Report_SundayHours_AreSundayHours_NotOvertime()
        {
            var report = await CreateService().GenerateMonthlyReportAsync(EmployeeId, 2025, 9);

            Assert.Equal(5, Day(report!, 7).SundayHours);
            Assert.Equal(0, Day(report!, 7).OvertimeHours);
            Assert.Equal(5, report!.TotalSundayHours);
        }

        [Fact]
        public async Task Report_HeritageDay_CountsAsPublicHoliday()
        {
            var report = await CreateService().GenerateMonthlyReportAsync(EmployeeId, 2025, 9);

            var heritageDay = Day(report!, 24);
            Assert.True(heritageDay.IsPublicHoliday);
            Assert.Equal(6, heritageDay.PublicHolidayHours);
            Assert.Equal(0, heritageDay.OvertimeHours);
            Assert.Contains("Heritage Day", heritageDay.Remarks);
        }

        [Fact]
        public async Task Report_WeekendWithoutRecord_IsWeekend_NotAbsent()
        {
            var report = await CreateService().GenerateMonthlyReportAsync(EmployeeId, 2025, 9);

            Assert.Equal("Weekend", Day(report!, 6).Remarks);
        }

        [Fact]
        public async Task Report_WeekTotal_IsShownOnSunday()
        {
            var report = await CreateService().GenerateMonthlyReportAsync(EmployeeId, 2025, 9);

            // Week Mon 1 - Sun 7: 10 + 8 + 5 = 23 hours
            Assert.Equal<double?>(23, Day(report!, 7).WeekTotalHours);
            Assert.Null(Day(report!, 1).WeekTotalHours);
        }

        [Fact]
        public async Task Report_MonthTotals_AddUp()
        {
            var report = await CreateService().GenerateMonthlyReportAsync(EmployeeId, 2025, 9);

            Assert.Equal(29, report!.TotalHoursWorked);   // 10 + 8 + 5 + 6
            Assert.Equal(6, report.TotalPublicHolidayHours);
        }

        [Fact]
        public async Task Report_UnknownEmployee_ReturnsNull()
        {
            var report = await CreateService().GenerateMonthlyReportAsync("nobody", 2025, 9);

            Assert.Null(report);
        }

        [Fact]
        public async Task Csv_ContainsEmployeeAndTotals()
        {
            var service = CreateService();
            var report = await service.GenerateMonthlyReportAsync(EmployeeId, 2025, 9);

            var csv = service.BuildCsv(report!);

            Assert.Contains("Employee Name:,Jane Doe", csv);
            Assert.Contains("Month:,September", csv);
            Assert.Contains("Days Present:,4", csv);
        }

        // ---------- fakes (in memory, no MongoDB) ----------

        public class FakeAttendanceRepository : IAttendanceRepository
        {
            private readonly List<Attendance> _records;

            public FakeAttendanceRepository(params Attendance[] records) => _records = records.ToList();

            public Task<List<Attendance>> GetByDateRangeAsync(DateTime from, DateTime to) =>
                Task.FromResult(_records.Where(a => a.AttendanceDate >= from && a.AttendanceDate <= to).ToList());

            public Task<List<Attendance>> GetAllAsync() => Task.FromResult(_records.ToList());
            public Task<Attendance?> GetByIdAsync(string id) => Task.FromResult(_records.FirstOrDefault(a => a.Id == id));
            public Task<List<Attendance>> GetByEmployeeIdAsync(string employeeId) =>
                Task.FromResult(_records.Where(a => a.EmployeeId == employeeId).ToList());
            public Task<Attendance?> GetByEmployeeAndDateAsync(string employeeId, DateTime date) =>
                Task.FromResult(_records.FirstOrDefault(a => a.EmployeeId == employeeId && a.AttendanceDate.Date == date.Date));
            public Task CreateAsync(Attendance attendance) { _records.Add(attendance); return Task.CompletedTask; }
            public Task UpdateAsync(Attendance attendance) => Task.CompletedTask;
            public Task DeleteAsync(string id) => Task.CompletedTask;
        }

        public class FakeEmployeeRepository : IEmployeeRepository
        {
            private readonly List<Employee> _employees;

            public FakeEmployeeRepository(params Employee[] employees) => _employees = employees.ToList();

            public Task<Employee?> GetByIdAsync(string id) => Task.FromResult(_employees.FirstOrDefault(e => e.Id == id));
            public Task<List<Employee>> GetAllAsync() => Task.FromResult(_employees.ToList());
            public Task CreateAsync(Employee employee) => Task.CompletedTask;
            public Task UpdateAsync(Employee employee) => Task.CompletedTask;
            public Task DeleteAsync(string id) => Task.CompletedTask;
            public Task<Employee?> GetByUserIdAsync(string userId) => Task.FromResult<Employee?>(null);
            public Task<Employee?> GetByEmailAsync(string email) => Task.FromResult<Employee?>(null);
            public Task<Employee?> GetByDeviceUserIdAsync(string deviceUserId) => Task.FromResult<Employee?>(null);
        }

        public class FakeDepartmentRepository : IDepartmentRepository
        {
            private readonly List<Department> _departments;

            public FakeDepartmentRepository(params Department[] departments) => _departments = departments.ToList();

            public Task<Department?> GetByIdAsync(string id) => Task.FromResult(_departments.FirstOrDefault(d => d.Id == id));
            public Task<List<Department>> GetAllAsync() => Task.FromResult(_departments.ToList());
            public Task CreateAsync(Department department) => Task.CompletedTask;
            public Task UpdateAsync(Department department) => Task.CompletedTask;
            public Task DeleteAsync(string id) { _departments.RemoveAll(d => d.Id == id); return Task.CompletedTask; }
        }
    }
}