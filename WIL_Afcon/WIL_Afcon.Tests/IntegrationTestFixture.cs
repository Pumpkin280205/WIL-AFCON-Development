using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using MongoDB.Driver;
using Testcontainers.MongoDb;
using Xunit;

namespace WIL_Afcon.Tests
{
    // Integration tests run both apps in memory (WebApplicationFactory) against a
    // real, throwaway MongoDB started in Docker (Testcontainers). Docker must be
    // running locally; GitHub's ubuntu runners have it by default.
    //
    // The "Testing" environment means no user-secrets or Atlas connection string
    // are loaded, so the tests can never touch the real database.
    public class IntegrationTestFixture : IAsyncLifetime
    {
        public const string ApiKey = "integration-test-api-key";
        public const string DeviceToken = "integration-test-device-token";

        public const string AdminEmail = "admin@tests.local";
        public const string AdminPassword = "Admin-Test-123!";
        public const string EmployeeEmail = "employee@tests.local";
        public const string EmployeePassword = "Employee-Test-123!";

        private readonly MongoDbContainer _mongo = new MongoDbBuilder("mongo:8.0")
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
                });
        }

        public async Task DisposeAsync()
        {
            await Api.DisposeAsync();
            await Web.DisposeAsync();
            await _mongo.DisposeAsync();
        }

        // UseSetting (rather than ConfigureAppConfiguration) so the values are
        // visible while Program.cs is still registering services.
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

    // One MongoDB container shared by every integration test class
    [CollectionDefinition(Name)]
    public class IntegrationCollection : ICollectionFixture<IntegrationTestFixture>
    {
        public const string Name = "Integration";
    }
}
