using WebModels = WIL_Afcon.Models;
using WebRepos = WIL_Afcon.Repositories.Interfaces;
using WebServices = WIL_Afcon.Services;
using ApiModels = WIL_Afcon.API.Models;
using ApiRepos = WIL_Afcon.API.Repositories.Interfaces;
using ApiServices = WIL_Afcon.API.Services;
using Xunit;

namespace WIL_Afcon.Tests
{
    // Device User ID rule: two employees can't share the same Device User ID.
    //  - Web app: Services/EmployeeService.cs (CreateAsync / UpdateAsync)
    //  - API:     Services/EmployeeService.cs (UpdateDeviceBindingAsync)
    public class DeviceUserIdTests
    {
        // WEB APP - EmployeeService
        
        private static WebServices.EmployeeService CreateWebService(FakeWebEmployeeRepository repo)
        {
            // HttpClient is only used by DeactivateAsync (see EmployeeDeactivationTests).
            return new WebServices.EmployeeService(repo, new HttpClient());
        }

        private static WebModels.Employee WebEmployee(string id, string? deviceUserId) => new()
        {
            Id = id,
            FirstName = "Test",
            LastName = id,
            DeviceUserId = deviceUserId
        };

        [Fact]
        public async Task Create_WithDeviceIdAlreadyUsed_IsRejected()
        {
            var repo = new FakeWebEmployeeRepository(WebEmployee("emp-1", "1001"));
            var service = CreateWebService(repo);

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.CreateAsync(WebEmployee("emp-2", "1001")));

            Assert.Single(repo.Employees); // the second employee was NOT saved
        }

        [Fact]
        public async Task Create_WithUniqueDeviceId_IsSaved()
        {
            var repo = new FakeWebEmployeeRepository(WebEmployee("emp-1", "1001"));
            var service = CreateWebService(repo);

            await service.CreateAsync(WebEmployee("emp-2", "1002"));

            Assert.Equal(2, repo.Employees.Count);
        }

        [Fact]
        public async Task Create_WithoutDeviceId_IsAllowed_ForManyEmployees()
        {
            var repo = new FakeWebEmployeeRepository(WebEmployee("emp-1", null));
            var service = CreateWebService(repo);

            await service.CreateAsync(WebEmployee("emp-2", null));
            await service.CreateAsync(WebEmployee("emp-3", ""));

            Assert.Equal(3, repo.Employees.Count);
        }

        [Fact]
        public async Task Update_ToAnotherEmployeesDeviceId_IsRejected()
        {
            var repo = new FakeWebEmployeeRepository(
                WebEmployee("emp-1", "1001"),
                WebEmployee("emp-2", "1002"));
            var service = CreateWebService(repo);

            var edited = WebEmployee("emp-2", "1001");

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateAsync(edited));
            Assert.Equal(0, repo.UpdateCalls);
        }

        [Fact]
        public async Task Update_KeepingOwnDeviceId_IsAllowed()
        {
            var repo = new FakeWebEmployeeRepository(WebEmployee("emp-1", "1001"));
            var service = CreateWebService(repo);

            await service.UpdateAsync(WebEmployee("emp-1", "1001"));

            Assert.Equal(1, repo.UpdateCalls);
        }

        // =====================================================================
        // API - EmployeeService.UpdateDeviceBindingAsync
        // =====================================================================

        private static ApiModels.Employee ApiEmployee(string id, string? deviceUserId) => new()
        {
            Id = id,
            DeviceUserId = deviceUserId
        };

        [Fact]
        public async Task Api_BindDeviceIdUsedByAnotherEmployee_IsRejected()
        {
            var repo = new FakeApiEmployeeRepository(
                ApiEmployee("emp-1", "1001"),
                ApiEmployee("emp-2", null));
            var service = new ApiServices.EmployeeService(repo);

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.UpdateDeviceBindingAsync("emp-2", "1001"));

            Assert.Null(repo.Employees.Single(e => e.Id == "emp-2").DeviceUserId);
        }

        [Fact]
        public async Task Api_BindDeviceId_IsTrimmedAndSaved()
        {
            var repo = new FakeApiEmployeeRepository(ApiEmployee("emp-1", null));
            var service = new ApiServices.EmployeeService(repo);

            var result = await service.UpdateDeviceBindingAsync("emp-1", "  1001  ");

            Assert.Equal("1001", result!.DeviceUserId);
        }

        [Fact]
        public async Task Api_BindDeviceId_UnknownEmployee_ReturnsNull()
        {
            var service = new ApiServices.EmployeeService(new FakeApiEmployeeRepository());

            var result = await service.UpdateDeviceBindingAsync("nobody", "1001");

            Assert.Null(result);
        }

    

        public class FakeWebEmployeeRepository : WebRepos.IEmployeeRepository
        {
            public List<WebModels.Employee> Employees { get; }
            public int UpdateCalls { get; private set; }

            public FakeWebEmployeeRepository(params WebModels.Employee[] employees) =>
                Employees = employees.ToList();

            public Task<WebModels.Employee?> GetByDeviceUserIdAsync(string deviceUserId) =>
                Task.FromResult(Employees.FirstOrDefault(e => e.DeviceUserId == deviceUserId));

            public Task CreateAsync(WebModels.Employee employee)
            {
                Employees.Add(employee);
                return Task.CompletedTask;
            }

            public Task UpdateAsync(WebModels.Employee employee)
            {
                UpdateCalls++;
                return Task.CompletedTask;
            }

            public Task<List<WebModels.Employee>> GetAllAsync() => Task.FromResult(Employees.ToList());
            public Task<WebModels.Employee?> GetByIdAsync(string id) =>
                Task.FromResult(Employees.FirstOrDefault(e => e.Id == id));
            public Task DeleteAsync(string id) => Task.CompletedTask;
            public Task<WebModels.Employee?> GetByUserIdAsync(string userId) =>
                Task.FromResult(Employees.FirstOrDefault(e => e.UserId == userId));
            public Task<WebModels.Employee?> GetByEmailAsync(string email) =>
                Task.FromResult(Employees.FirstOrDefault(e => e.Email == email));
        }

        public class FakeApiEmployeeRepository : ApiRepos.IEmployeeRepository
        {
            public List<ApiModels.Employee> Employees { get; }

            public FakeApiEmployeeRepository(params ApiModels.Employee[] employees) =>
                Employees = employees.ToList();

            public Task<ApiModels.Employee?> GetByIdAsync(string id) =>
                Task.FromResult(Employees.FirstOrDefault(e => e.Id == id));

            public Task<ApiModels.Employee?> GetByDeviceUserIdAsync(string deviceUserId) =>
                Task.FromResult(Employees.FirstOrDefault(e => e.DeviceUserId == deviceUserId));

            public Task UpdateAsync(ApiModels.Employee employee) => Task.CompletedTask;
            public Task<List<ApiModels.Employee>> GetAllAsync() => Task.FromResult(Employees.ToList());
            public Task CreateAsync(ApiModels.Employee employee) { Employees.Add(employee); return Task.CompletedTask; }
            public Task DeleteAsync(string id) => Task.CompletedTask;
            public Task<ApiModels.Employee?> GetByUserIdAsync(string userId) =>
                Task.FromResult(Employees.FirstOrDefault(e => e.UserId == userId));
            public Task<ApiModels.Employee?> GetByEmailAsync(string email) =>
                Task.FromResult(Employees.FirstOrDefault(e => e.Email == email));
            public Task<ApiModels.Employee?> GetByEmployeeNumberAsync(string employeeNumber) =>
                Task.FromResult(Employees.FirstOrDefault(e => e.EmployeeNumber == employeeNumber));
        }
    }
}