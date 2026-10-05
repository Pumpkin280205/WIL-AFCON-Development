using System.Net;
using WIL_Afcon.Models;
using WIL_Afcon.Models.Enums;
using WIL_Afcon.Services;
using Xunit;
using static WIL_Afcon.Tests.DeviceUserIdTests;

namespace WIL_Afcon.Tests
{
    // Deactivating an employee (web app Services/EmployeeService.cs DeactivateAsync):
    //  - the employee is always marked inactive, even if the device is offline
    //  - the biometric device removal is attempted when a Device User ID is set
    //  - the outcome says exactly what happened
    public class EmployeeDeactivationTests
    {
        private static Employee ActiveEmployee(string? deviceUserId) => new()
        {
            Id = "emp-1",
            FirstName = "Test",
            LastName = "Employee",
            DeviceUserId = deviceUserId,
            IsActive = true
        };

        private static EmployeeService CreateService(
            FakeWebEmployeeRepository repo,
            FakeDeviceApi api) =>
            new(repo, new HttpClient(api) { BaseAddress = new Uri("http://api.test/") });

        [Fact]
        public async Task UnknownEmployee_ReturnsNotFound_AndSavesNothing()
        {
            var repo = new FakeWebEmployeeRepository();
            var api = new FakeDeviceApi(HttpStatusCode.OK);

            var outcome = await CreateService(repo, api).DeactivateAsync("missing");

            Assert.Equal(DeactivationOutcome.NotFound, outcome);
            Assert.Equal(0, repo.UpdateCalls);
            Assert.Equal(0, api.Calls);
        }

        [Fact]
        public async Task WithoutDeviceId_IsDeactivated_WithoutCallingTheDevice()
        {
            var employee = ActiveEmployee(deviceUserId: null);
            var repo = new FakeWebEmployeeRepository(employee);
            var api = new FakeDeviceApi(HttpStatusCode.OK);

            var outcome = await CreateService(repo, api).DeactivateAsync(employee.Id);

            Assert.Equal(DeactivationOutcome.Deactivated, outcome);
            Assert.False(employee.IsActive);
            Assert.Equal(1, repo.UpdateCalls);
            Assert.Equal(0, api.Calls);
        }

        [Fact]
        public async Task WithDeviceId_IsDeactivated_AndRemovedFromDevice()
        {
            var employee = ActiveEmployee(deviceUserId: "1001");
            var repo = new FakeWebEmployeeRepository(employee);
            var api = new FakeDeviceApi(HttpStatusCode.OK);

            var outcome = await CreateService(repo, api).DeactivateAsync(employee.Id);

            Assert.Equal(DeactivationOutcome.DeactivatedAndRemovedFromDevice, outcome);
            Assert.False(employee.IsActive);
            Assert.Equal(HttpMethod.Delete, api.LastRequest!.Method);
            Assert.EndsWith("/api/attendance/device/employee/1001", api.LastRequest.RequestUri!.AbsolutePath);
        }

        [Fact]
        public async Task DeviceRejectsRemoval_EmployeeIsStillDeactivated()
        {
            var employee = ActiveEmployee(deviceUserId: "1001");
            var repo = new FakeWebEmployeeRepository(employee);
            var api = new FakeDeviceApi(HttpStatusCode.BadGateway);

            var outcome = await CreateService(repo, api).DeactivateAsync(employee.Id);

            Assert.Equal(DeactivationOutcome.DeactivatedButDeviceRemovalFailed, outcome);
            Assert.False(employee.IsActive);
            Assert.Equal(1, repo.UpdateCalls);
        }

        [Fact]
        public async Task ApiUnreachable_EmployeeIsStillDeactivated()
        {
            var employee = ActiveEmployee(deviceUserId: "1001");
            var repo = new FakeWebEmployeeRepository(employee);
            var api = new FakeDeviceApi(throwConnectionError: true);

            var outcome = await CreateService(repo, api).DeactivateAsync(employee.Id);

            Assert.Equal(DeactivationOutcome.DeactivatedButDeviceRemovalFailed, outcome);
            Assert.False(employee.IsActive);
        }

        // Stands in for the REST API so no real HTTP call is made
        public class FakeDeviceApi : HttpMessageHandler
        {
            private readonly HttpStatusCode _status;
            private readonly bool _throwConnectionError;

            public int Calls { get; private set; }
            public HttpRequestMessage? LastRequest { get; private set; }

            public FakeDeviceApi(HttpStatusCode status = HttpStatusCode.OK, bool throwConnectionError = false)
            {
                _status = status;
                _throwConnectionError = throwConnectionError;
            }

            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                Calls++;
                LastRequest = request;

                if (_throwConnectionError)
                {
                    throw new HttpRequestException("Connection refused");
                }

                return Task.FromResult(new HttpResponseMessage(_status));
            }
        }
    }
}
