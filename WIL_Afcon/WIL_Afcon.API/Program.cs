using WIL_Afcon.API.Configuration;
using WIL_Afcon.API.Data;
using WIL_Afcon.API.Repositories;
using WIL_Afcon.API.Repositories.Interfaces;
using WIL_Afcon.API.Services;
using WIL_Afcon.API.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddSingleton<MongoDbContext>();

// Repositories
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
builder.Services.AddScoped<IAttendanceRepository, AttendanceRepository>();

// Services
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<IAttendanceService, AttendanceService>();

// Biometric terminal (Hikvision DS-K1T67MF) integration
builder.Services.Configure<HikvisionDeviceOptions>(
    builder.Configuration.GetSection(HikvisionDeviceOptions.SectionName));
builder.Services.AddSingleton<DeviceHealthTracker>();
builder.Services.AddScoped<IDeviceEventService, DeviceEventService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run();