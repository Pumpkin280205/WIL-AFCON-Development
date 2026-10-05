using WIL_Afcon.Data;

using WIL_Afcon.Repositories;
using WIL_Afcon.Repositories.Interfaces;
using WIL_Afcon.Services;
using WIL_Afcon.Services.Interfaces;
using AspNetCore.Identity.Mongo;
using AspNetCore.Identity.Mongo.Model;
using WIL_Afcon.Security;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<MongoDbContext>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Liveness probe for the hosting platform (Render health checks)
builder.Services.AddHealthChecks();

// Rate limit login/register posts per IP to slow down password guessing
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("login", context => RateLimitPartition.GetFixedWindowLimiter(
        partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        factory: _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = builder.Configuration.GetValue("RateLimiting:LoginPermitLimitPerMinute", 10),
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0
        }));
});


//authentication
builder.Services.AddIdentityMongoDbProvider<MongoUser, MongoRole>(
    identity =>
    {
        identity.Password.RequiredLength = 8;
        identity.User.RequireUniqueEmail = true;
    },
    mongo =>
    {
        mongo.ConnectionString =
            builder.Configuration["MongoDbSettings:ConnectionString"]!;

        mongo.RolesCollection = "Roles";
        mongo.UsersCollection = "Users";
        mongo.MigrationCollection = "_Migrations";
    });

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});

// Repositories
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
builder.Services.AddScoped<IAttendanceRepository, AttendanceRepository>();
builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();

// Services
// Calls to WIL_Afcon.API go through typed HttpClients that share the API's
// base address and the X-Api-Key header (both from configuration).
void ConfigureApiClient(HttpClient client)
{
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"]!);

    var apiKey = builder.Configuration["ApiSettings:ApiKey"];
    if (!string.IsNullOrWhiteSpace(apiKey))
    {
        client.DefaultRequestHeaders.Add("X-Api-Key", apiKey);
    }
}

builder.Services.AddHttpClient<IEmployeeService, EmployeeService>(ConfigureApiClient);
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<IAttendanceService, AttendanceService>();
builder.Services.AddScoped<IAttendanceReportService, AttendanceReportService>(); // Attendance report feature
builder.Services.AddHttpClient<IAttendanceRequestService, AttendanceRequestService>(ConfigureApiClient);
builder.Services.AddScoped<IDocumentService, DocumentService>();



var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    await IdentitySetup.InitializeAsync(scope.ServiceProvider);
}

await app.Services.GetRequiredService<MongoDbContext>()
    .EnsureIndexesAsync(app.Logger);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// HTTPS: when hosted, the platform terminates TLS and forwards plain HTTP,
// so HSTS (above) plus a Secure-only auth cookie enforce HTTPS for browsers.
app.UseSecurityHeaders(app.Configuration, app.Environment);

app.UseRouting();

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapHealthChecks("/health");


app.Run();


// REFERENCES

// ============================================================

//

// AspNetCore.Identity.Mongo (2026) 'AspNetCore.Identity.Mongo',

// GitHub.

// Available at: https://github.com/matteofabbri/AspNetCore.Identity.Mongo

// (Accessed: 26 September 2026).

//

// Microsoft (2026) 'ASP.NET Core middleware', Microsoft Learn.

// Available at: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware

// (Accessed: 26 September 2026).

//

// Microsoft (2026) 'Dependency injection in ASP.NET Core', Microsoft Learn.

// Available at: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection

// (Accessed: 26 September 2026).

//

// Microsoft (2026) 'Enforce HTTPS in ASP.NET Core', Microsoft Learn.

// Available at: https://learn.microsoft.com/en-us/aspnet/core/security/enforcing-ssl

// (Accessed: 26 September 2026).

//

// Microsoft (2026) 'Error handling in ASP.NET Core', Microsoft Learn.

// Available at: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/error-handling

// (Accessed: 26 September 2026).

//

// Microsoft (2026) 'WebApplicationBuilder Class', Microsoft Learn.

// Available at: https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.builder.webapplicationbuilder

// (Accessed: 26 September 2026).

//

// ===========================================================
