using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Driver;
using WIL_Afcon.API.Data;
using WIL_Afcon.API.Models;
using WIL_Afcon.API.Security;
using Xunit;

namespace WIL_Afcon.Tests
{
    // API tests: API key, device token, device-binding conflicts, unique indexes, problem+json errors
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
        public async Task CreateEmployee_WithDuplicateEmail_Returns409Problem()
        {
            var client = ClientWithKey();
            var first = NewEmployee();

            Assert.Equal(HttpStatusCode.Created,
                (await client.PostAsJsonAsync("/api/employees", first)).StatusCode);

            // Same email, different employee: the unique index rejects it and the
            // global exception handler turns that into 409 instead of a 500
            var duplicate = NewEmployee();
            duplicate.Email = first.Email;

            var response = await client.PostAsJsonAsync("/api/employees", duplicate);

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
            await AssertProblemAsync(response, 409);
        }

        [Fact]
        public async Task GetUnknownEmployee_Returns404Problem()
        {
            var response = await ClientWithKey()
                .GetAsync($"/api/employees/{ObjectId.GenerateNewId()}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            await AssertProblemAsync(response, 404);
        }

        [Fact]
        public async Task MissingApiKey_Returns401Problem()
        {
            var response = await ClientWithKey(apiKey: null).GetAsync("/api/employees");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            await AssertProblemAsync(response, 401);
        }

        [Fact]
        public async Task EndBreak_WithoutClockingIn_Returns400Problem()
        {
            // A brand-new id has no attendance today, so this breaks a rule at any time of day
            var response = await ClientWithKey()
                .PostAsync($"/api/attendance/{ObjectId.GenerateNewId()}/break/end", null);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            await AssertProblemAsync(response, 400);
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

            // Partial unique index: null/empty values are allowed to repeat
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
            // Empty POST = device keep-alive; needs the device token, not the API key
            var response = await ClientWithKey(apiKey: null).PostAsync(
                $"/api/attendance/hikvision-event?token={IntegrationTestFixture.DeviceToken}",
                new StringContent(string.Empty));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        // Every API error should use the RFC 7807 problem+json shape
        private static async Task AssertProblemAsync(HttpResponseMessage response, int expectedStatus)
        {
            Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

            var problem = await response.Content.ReadFromJsonAsync<JsonElement>();

            Assert.Equal(expectedStatus, problem.GetProperty("status").GetInt32());
            Assert.False(string.IsNullOrWhiteSpace(problem.GetProperty("title").GetString()));
        }

        private IMongoCollection<Employee> EmployeesCollection()
        {
            // Start the API first so its indexes exist
            _ = _fixture.Api.Server;

            return _fixture.Api.Services
                .GetRequiredService<MongoDbContext>()
                .Database.GetCollection<Employee>("Employees");
        }
    }
}
