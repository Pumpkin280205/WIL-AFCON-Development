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
