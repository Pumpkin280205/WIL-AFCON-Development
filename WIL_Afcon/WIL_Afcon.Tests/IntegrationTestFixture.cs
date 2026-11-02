using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using MongoDB.Driver;
using Testcontainers.MongoDb;
using Xunit;

namespace WIL_Afcon.Tests
{
    // Runs both apps in memory against a throwaway MongoDB in Docker (Docker must be running).
    // "Testing" environment: no user-secrets, so the real database is never used.
    public class IntegrationTestFixture : IAsyncLifetime
    {
        public const string ApiKey = "integration-test-api-key";
        public const string DeviceToken = "integration-test-device-token";

        public const string AdminEmail = "admin@tests.local";
        public const string AdminPassword = "Admin-Test-123!";
        public const string EmployeeEmail = "employee@tests.local";
        public const string EmployeePassword = "Employee-Test-123!";

        // shortcut: 7.0 because mongo 8.0+ won't start on Linux kernel 6.19+ (GitHub runners);
        // move back to 8.x once MongoDB ships a build that supports those kernels (SERVER-121912)
        private readonly MongoDbContainer _mongo = new MongoDbBuilder("mongo:7.0")
            .Build();

        public WebApplicationFactory<WIL_Afcon.API.Data.MongoDbContext> Api { get; private set; } = null!;

        public WebApplicationFactory<WIL_Afcon.Data.MongoDbContext> Web { get; private set; } = null!;

        public async Task InitializeAsync()
        {
            await _mongo.StartAsync();

            Api = new WebApplicationFactory<WIL_Afcon.API.Data.MongoDbContext>()
                .WithWebHostBuilder(builder =>
                {
                    UseTestDatabase(builder, "wil_afcon_api_tests");
                    builder.UseSetting("ApiSecurity:ApiKey", ApiKey);
                    builder.UseSetting("HikvisionDevice:InboundToken", DeviceToken);
                });

            Web = new WebApplicationFactory<WIL_Afcon.Data.MongoDbContext>()
                .WithWebHostBuilder(builder =>
                {
                    UseTestDatabase(builder, "wil_afcon_web_tests");
                    builder.UseSetting("ApiSettings:BaseUrl", "http://localhost/");
                    builder.UseSetting("ApiSettings:ApiKey", ApiKey);
                    builder.UseSetting("SeedAccounts:AdminEmail", AdminEmail);
                    builder.UseSetting("SeedAccounts:AdminPassword", AdminPassword);
                    builder.UseSetting("SeedAccounts:EmployeeEmail", EmployeeEmail);
                    builder.UseSetting("SeedAccounts:EmployeePassword", EmployeePassword);
                    // High limit so repeated test logins aren't blocked
                    builder.UseSetting("RateLimiting:LoginPermitLimitPerMinute", "1000");
                });
        }

        public async Task DisposeAsync()
        {
            await Api.DisposeAsync();
            await Web.DisposeAsync();
            await _mongo.DisposeAsync();
        }

        // UseSetting so values are visible while Program.cs registers services
        private void UseTestDatabase(IWebHostBuilder builder, string databaseName)
        {
            var connectionString = new MongoUrlBuilder(_mongo.GetConnectionString())
            {
                DatabaseName = databaseName,
                AuthenticationSource = "admin"
            }.ToString();

            builder.UseEnvironment("Testing");
            builder.UseSetting("MongoDbSettings:ConnectionString", connectionString);
            builder.UseSetting("MongoDbSettings:DatabaseName", databaseName);
        }
    }

    // Shares one MongoDB container across integration test classes
    [CollectionDefinition(Name)]
    public class IntegrationCollection : ICollectionFixture<IntegrationTestFixture>
    {
        public const string Name = "Integration";
    }
}
