using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Driver;
using WIL_Afcon.API.Data;
using WIL_Afcon.API.Models;
using WIL_Afcon.API.Security;
using Xunit;

namespace WIL_Afcon.Tests
{
    // End-to-end checks on WIL_Afcon.API: API key protection, the Hikvision
    // webhook token, device-binding conflicts and the MongoDB unique indexes.
    [Collection(IntegrationCollection.Name)]
    public class ApiIntegrationTests
    {
        private readonly IntegrationTestFixture _fixture;

        public ApiIntegrationTests(IntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        private HttpClient ClientWithKey(string? apiKey = IntegrationTestFixture.ApiKey)
        {
            var client = _fixture.Api.CreateClient();
            if (apiKey != null)
            {
                client.DefaultRequestHeaders.Add(ApiKeyMiddleware.HeaderName, apiKey);
            }
            return client;
        }

        private static Employee NewEmployee(string? deviceUserId = null) => new()
        {
            EmployeeNumber = $"EMP-{Guid.NewGuid():N}",
            FirstName = "Test",
            LastName = "Employee",
            Email = $"{Guid.NewGuid():N}@tests.local",
            DepartmentId = ObjectId.GenerateNewId().ToString(),
            DeviceUserId = deviceUserId
        };

        [Fact]
        public async Task Health_IsReachableWithoutApiKey()
        {
            var response = await ClientWithKey(apiKey: null).GetAsync("/health");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("wrong-key")]
        public async Task ProtectedEndpoint_WithoutValidApiKey_Returns401(string? apiKey)
        {
            var response = await ClientWithKey(apiKey).GetAsync("/api/employees");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task ProtectedEndpoint_WithApiKey_Returns200()
        {
            var response = await ClientWithKey().GetAsync("/api/employees");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Responses_IncludeSecurityHeaders()
        {
            var response = await ClientWithKey().GetAsync("/api/employees");

            Assert.Equal("nosniff", response.Headers.GetValues("X-Content-Type-Options").Single());
        }

        [Fact]
        public async Task CreateEmployee_ThenGetById_ReturnsEmployee()
        {
            var client = ClientWithKey();

            var create = await client.PostAsJsonAsync("/api/employees", NewEmployee());
            Assert.Equal(HttpStatusCode.Created, create.StatusCode);

            var created = await create.Content.ReadFromJsonAsync<Employee>();
            var fetched = await client.GetFromJsonAsync<Employee>($"/api/employees/{created!.Id}");

            Assert.Equal(created.EmployeeNumber, fetched!.EmployeeNumber);
        }

        [Fact]
        public async Task BindingDeviceIdAlreadyInUse_Returns409()
        {
            var client = ClientWithKey();
            var deviceUserId = Guid.NewGuid().ToString("N")[..8];

            var first = await client.PostAsJsonAsync("/api/employees", NewEmployee(deviceUserId));
            Assert.Equal(HttpStatusCode.Created, first.StatusCode);

            var second = await (await client.PostAsJsonAsync("/api/employees", NewEmployee()))
                .Content.ReadFromJsonAsync<Employee>();

            var bind = await client.PutAsJsonAsync(
                $"/api/employees/{second!.Id}/device-binding",
                new { deviceUserId });

            Assert.Equal(HttpStatusCode.Conflict, bind.StatusCode);
        }

        [Fact]
        public async Task DuplicateDeviceUserId_IsRejectedByUniqueIndex()
        {
            var employees = EmployeesCollection();
            var deviceUserId = Guid.NewGuid().ToString("N")[..8];

            await employees.InsertOneAsync(NewEmployee(deviceUserId));

            var ex = await Assert.ThrowsAsync<MongoWriteException>(
                () => employees.InsertOneAsync(NewEmployee(deviceUserId)));

            Assert.Equal(ServerErrorCategory.DuplicateKey, ex.WriteError.Category);
        }

        [Fact]
        public async Task EmployeesWithoutDeviceUserId_DoNotCollide()
        {
            var employees = EmployeesCollection();

            // The unique index is partial, so unbound employees (null or "") are allowed
            await employees.InsertOneAsync(NewEmployee(deviceUserId: null));
            await employees.InsertOneAsync(NewEmployee(deviceUserId: null));
            await employees.InsertOneAsync(NewEmployee(deviceUserId: ""));
            await employees.InsertOneAsync(NewEmployee(deviceUserId: ""));
        }

        [Fact]
        public async Task HikvisionEvent_WithoutDeviceToken_Returns401()
        {
            var response = await ClientWithKey(apiKey: null)
                .PostAsync("/api/attendance/hikvision-event", new StringContent(string.Empty));

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task HikvisionKeepAlive_WithDeviceToken_Returns200()
        {
            // An empty POST is the terminal's keep-alive; no API key is needed,
            // only the device token.
            var response = await ClientWithKey(apiKey: null).PostAsync(
                $"/api/attendance/hikvision-event?token={IntegrationTestFixture.DeviceToken}",
                new StringContent(string.Empty));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private IMongoCollection<Employee> EmployeesCollection()
        {
            // Make sure the API (and its start-up index creation) has run first
            _ = _fixture.Api.Server;

            return _fixture.Api.Services
                .GetRequiredService<MongoDbContext>()
                .Database.GetCollection<Employee>("Employees");
        }
    }
}
