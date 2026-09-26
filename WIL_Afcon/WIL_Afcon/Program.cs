using WIL_Afcon.Data;

using WIL_Afcon.Repositories;
using WIL_Afcon.Repositories.Interfaces;
using WIL_Afcon.Services;
using WIL_Afcon.Services.Interfaces;
using AspNetCore.Identity.Mongo;
using AspNetCore.Identity.Mongo.Model;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<MongoDbContext>();


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

// Repositories
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
builder.Services.AddScoped<IAttendanceRepository, AttendanceRepository>();

// Services
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<IAttendanceService, AttendanceService>();



var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    await IdentitySetup.InitializeAsync(scope.ServiceProvider);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


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
