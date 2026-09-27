using WIL_Afcon.API.Configuration;
using WIL_Afcon.API.Data;
using WIL_Afcon.API.RealTime;
using WIL_Afcon.API.Repositories;
using WIL_Afcon.API.Repositories.Interfaces;
using WIL_Afcon.API.Services;
using WIL_Afcon.API.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSignalR();

builder.Services.AddSingleton<MongoDbContext>();

// Repositories
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
builder.Services.AddScoped<IAttendanceRepository, AttendanceRepository>();
builder.Services.AddScoped<IAttendanceRequestRepository, AttendanceRequestRepository>();

// Services
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<IAttendanceService, AttendanceService>();
builder.Services.AddScoped<IHikvisionService, HikvisionService>();
builder.Services.AddScoped<IAttendanceRequestService, AttendanceRequestService>();
builder.Services.AddScoped<IAttendanceDocumentService, AttendanceDocumentService>();

// Biometric terminal (Hikvision DS-K1T67MF) integration
builder.Services.Configure<HikvisionDeviceOptions>(
    builder.Configuration.GetSection(HikvisionDeviceOptions.SectionName));
builder.Services.AddSingleton<DeviceHealthTracker>();
builder.Services.AddScoped<IDeviceEventService, DeviceEventService>();

// Work-day rules for biometric scans (clock in / break / clock out)
builder.Services.Configure<WorkScheduleOptions>(
    builder.Configuration.GetSection(WorkScheduleOptions.SectionName));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins(
                "https://localhost:7073",
                "http://localhost:5084")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
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
app.MapHub<RealtimeAttendance>("/attendanceHub");

app.Run();