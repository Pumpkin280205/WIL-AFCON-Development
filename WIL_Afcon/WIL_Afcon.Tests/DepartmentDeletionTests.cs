using WIL_Afcon.Models;
using WIL_Afcon.Services;
using Xunit;
using static WIL_Afcon.Tests.AttendanceReportTests;

namespace WIL_Afcon.Tests
{
    // Deleting a department (web app Services/DepartmentService.cs DeleteAsync)
    // is refused while employees are still assigned to it.
    public class DepartmentDeletionTests
    {
        private static Department It() => new() { Id = "dep-1", DepartmentName = "IT" };

        [Fact]
        public async Task EmptyDepartment_IsDeleted()
        {
            var departments = new FakeDepartmentRepository(It());
            var service = new DepartmentService(departments, new FakeEmployeeRepository());

            Assert.Equal(0, await service.DeleteAsync("dep-1"));
            Assert.Empty(await departments.GetAllAsync());
        }

        [Fact]
        public async Task DepartmentWithEmployees_IsKept()
        {
            var departments = new FakeDepartmentRepository(It());
            var employees = new FakeEmployeeRepository(
                new Employee { Id = "e1", DepartmentId = "dep-1" },
                new Employee { Id = "e2", DepartmentId = "dep-1" },
                new Employee { Id = "e3", DepartmentId = "dep-2" });

            Assert.Equal(2, await new DepartmentService(departments, employees).DeleteAsync("dep-1"));
            Assert.Single(await departments.GetAllAsync());
        }
    }
}
