using System.Net;
using System.Text.RegularExpressions;
using AspNetCore.Identity.Mongo.Model;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace WIL_Afcon.Tests
{
    // Web app tests: role access, login, anti-forgery, lockout, rate limiting, headers
    [Collection(IntegrationCollection.Name)]
    public class WebAppIntegrationTests
    {
        private readonly IntegrationTestFixture _fixture;

        public WebAppIntegrationTests(IntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        // HTTPS for the Secure auth cookie; redirects not followed so they can be checked
        private HttpClient NewClient() => _fixture.Web.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false
        });

        [Fact]
        public async Task Health_Returns200()
        {
            var response = await NewClient().GetAsync("/health");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Theory]
        [InlineData("/Admin/Dashboard")]
        [InlineData("/Employee")]
        [InlineData("/Department")]
        [InlineData("/Attendance")]
        [InlineData("/AttendanceRequest")]
        public async Task ProtectedPage_WhenAnonymous_RedirectsToLogin(string path)
        {
            var response = await NewClient().GetAsync(path);

            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.StartsWith("/Account/Login", response.Headers.Location!.PathAndQuery);
        }

        [Fact]
        public async Task LoginPage_IsPublic()
        {
            var response = await NewClient().GetAsync("/Account/Login");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Login_WithoutAntiForgeryToken_Returns400()
        {
            var response = await NewClient().PostAsync("/Account/Login", new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["email"] = IntegrationTestFixture.AdminEmail,
                ["password"] = IntegrationTestFixture.AdminPassword
            }));

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Login_WithWrongPassword_ShowsError()
        {
            var response = await LoginAsync(NewClient(), IntegrationTestFixture.AdminEmail, "Not-The-Password-1!");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("Invalid email or password", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Admin_CanOpenAdminPages()
        {
            var client = NewClient();

            var login = await LoginAsync(client, IntegrationTestFixture.AdminEmail, IntegrationTestFixture.AdminPassword);
            Assert.Equal("/Admin/Dashboard", login.Headers.Location!.OriginalString);

            var response = await client.GetAsync("/Department");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Employee_IsDeniedAdminPages()
        {
            var client = NewClient();

            var login = await LoginAsync(client, IntegrationTestFixture.EmployeeEmail, IntegrationTestFixture.EmployeePassword);
            Assert.Equal("/Employee/Dashboard", login.Headers.Location!.OriginalString);

            var response = await client.GetAsync("/Department");

            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.StartsWith("/Account/AccessDenied", response.Headers.Location!.PathAndQuery);
        }

        [Fact]
        public async Task Account_IsLockedAfterRepeatedFailedLogins()
        {
            // Dedicated user so other tests aren't locked out
            var email = $"lockout-{Guid.NewGuid():N}@tests.local";
            const string password = "Lockout-Test-123!";

            using (var scope = _fixture.Web.Services.CreateScope())
            {
                var users = scope.ServiceProvider.GetRequiredService<UserManager<MongoUser>>();
                var created = await users.CreateAsync(new MongoUser { UserName = email, Email = email }, password);
                Assert.True(created.Succeeded);
                await users.AddToRoleAsync((await users.FindByEmailAsync(email))!, "Employee");
            }

            var client = NewClient();

            // Identity locks after 5 failed attempts
            for (var i = 0; i < 5; i++)
            {
                await LoginAsync(client, email, "Wrong-Password-1!");
            }

            // Correct password is now refused
            var response = await LoginAsync(client, email, password);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Null(response.Headers.Location);
        }

        [Fact]
        public async Task Login_IsRateLimitedPerClient()
        {
            // App instance with a limit of 2 login posts per minute
            using var factory = _fixture.Web.WithWebHostBuilder(builder =>
                builder.UseSetting("RateLimiting:LoginPermitLimitPerMinute", "2"));

            var client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost"),
                AllowAutoRedirect = false
            });

            await LoginAsync(client, "nobody@tests.local", "Wrong-Password-1!");
            await LoginAsync(client, "nobody@tests.local", "Wrong-Password-1!");
            // Unknown email, so no account gets locked
            var third = await LoginAsync(client, "nobody@tests.local", "Wrong-Password-1!");

            Assert.Equal(HttpStatusCode.TooManyRequests, third.StatusCode);
        }

        [Fact]
        public async Task Responses_IncludeSecurityHeaders()
        {
            var response = await NewClient().GetAsync("/Account/Login");

            Assert.Equal("nosniff", response.Headers.GetValues("X-Content-Type-Options").Single());
            Assert.Equal("DENY", response.Headers.GetValues("X-Frame-Options").Single());
            Assert.True(response.Headers.Contains("Content-Security-Policy"));
        }

        // Gets the anti-forgery token, then posts the login form
        private static async Task<HttpResponseMessage> LoginAsync(HttpClient client, string email, string password)
        {
            var page = await client.GetStringAsync("/Account/Login");
            var token = Regex.Match(page, "name=\"__RequestVerificationToken\" type=\"hidden\" value=\"([^\"]+)\"")
                .Groups[1].Value;

            return await client.PostAsync("/Account/Login", new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["email"] = email,
                ["password"] = password,
                ["__RequestVerificationToken"] = token
            }));
        }
    }
}
