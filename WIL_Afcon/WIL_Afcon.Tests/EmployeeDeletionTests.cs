using WIL_Afcon.Models;
using WIL_Afcon.Services;
using Xunit;

namespace WIL_Afcon.Tests
{
    // Deleting an employee (web app Services/EmployeeDeletionService.cs) only goes
    // ahead when the admin retypes both the Device User ID and the full name.
    public class EmployeeDeletionTests
    {
        private static Employee Jane(string? deviceUserId = "1001") => new()
        {
            Id = "emp-1",
            EmployeeNumber = "EMP-007",
            FirstName = "Jane",
            LastName = "Doe",
            DeviceUserId = deviceUserId
        };

        [Theory]
        [InlineData("1001", "Jane Doe")]
        [InlineData(" 1001 ", "  jane   DOE ")]
        public void MatchingIdAndName_IsConfirmed(string id, string name)
        {
            Assert.True(EmployeeDeletionService.ConfirmationMatches(Jane(), id, name));
        }

        [Theory]
        [InlineData("1002", "Jane Doe")]  // wrong device id
        [InlineData("1001", "Jane Smith")] // wrong name
        [InlineData("1001", "")]           // name missing
        [InlineData(null, "Jane Doe")]     // id missing
        [InlineData("EMP-007", "Jane Doe")] // employee number when a device id exists
        public void AnyMismatch_IsRejected(string? id, string name)
        {
            Assert.False(EmployeeDeletionService.ConfirmationMatches(Jane(), id, name));
        }

        [Fact]
        public void WithoutDeviceId_EmployeeNumberIsUsed()
        {
            Assert.True(EmployeeDeletionService.ConfirmationMatches(Jane(deviceUserId: null), "EMP-007", "Jane Doe"));
            Assert.False(EmployeeDeletionService.ConfirmationMatches(Jane(deviceUserId: null), "", "Jane Doe"));
        }
    }
}
