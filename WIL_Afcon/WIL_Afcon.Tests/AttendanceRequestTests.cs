using WIL_Afcon.API.Models;
using WIL_Afcon.API.Models.enums;
using WIL_Afcon.API.Repositories.Interfaces;
using WIL_Afcon.API.Services;
using Xunit;

namespace WIL_Afcon.Tests
{
    // Requests approve / reject logic (WIL_Afcon.API -> Services/AttendanceRequestService.cs)
    public class AttendanceRequestTests
    {
        private static AttendanceRequest PendingRequest(string id) => new()
        {
            Id = id,
            EmployeeId = "emp-1",
            RequestType = RequestType.Leave,
            RequestDate = new DateTime(2026, 3, 2),
            Status = AttendanceRequestStatus.Pending
        };

        [Fact]
        public async Task Create_AlwaysStartsAsPending()
        {
            var repo = new FakeRequestRepository();
            var service = new AttendanceRequestService(repo);

            // Even if someone tries to submit it as already accepted...
            var request = PendingRequest("req-1");
            request.Status = AttendanceRequestStatus.Accepted;

            var saved = await service.CreateAsync(request);

            Assert.Equal(AttendanceRequestStatus.Pending, saved.Status);
            Assert.Single(repo.Requests);
        }

        [Fact]
        public async Task Accept_SetsStatusCommentAndReviewer()
        {
            var repo = new FakeRequestRepository(PendingRequest("req-1"));
            var service = new AttendanceRequestService(repo);

            var ok = await service.AcceptAsync("req-1", "Approved, get well soon", "admin@afcon.co.za");

            var stored = repo.Requests.Single();
            Assert.True(ok);
            Assert.Equal(AttendanceRequestStatus.Accepted, stored.Status);
            Assert.Equal("Approved, get well soon", stored.AdminComment);
            Assert.Equal("admin@afcon.co.za", stored.ReviewedBy);
            Assert.NotNull(stored.ReviewedAt);
            Assert.Equal(1, repo.UpdateCalls);
        }

        [Fact]
        public async Task Reject_SetsStatusCommentAndReviewer()
        {
            var repo = new FakeRequestRepository(PendingRequest("req-1"));
            var service = new AttendanceRequestService(repo);

            var ok = await service.RejectAsync("req-1", "No supporting document", "admin@afcon.co.za");

            var stored = repo.Requests.Single();
            Assert.True(ok);
            Assert.Equal(AttendanceRequestStatus.Rejected, stored.Status);
            Assert.Equal("No supporting document", stored.AdminComment);
            Assert.NotNull(stored.ReviewedAt);
        }

        [Fact]
        public async Task Accept_UnknownRequest_ReturnsFalse_AndSavesNothing()
        {
            var repo = new FakeRequestRepository();
            var service = new AttendanceRequestService(repo);

            var ok = await service.AcceptAsync("missing", "x", "admin");

            Assert.False(ok);
            Assert.Equal(0, repo.UpdateCalls);
        }

        [Fact]
        public async Task Reject_UnknownRequest_ReturnsFalse_AndSavesNothing()
        {
            var repo = new FakeRequestRepository();
            var service = new AttendanceRequestService(repo);

            var ok = await service.RejectAsync("missing", "x", "admin");

            Assert.False(ok);
            Assert.Equal(0, repo.UpdateCalls);
        }

        [Fact]
        public async Task Accept_OnlyChangesTheChosenRequest()
        {
            var repo = new FakeRequestRepository(PendingRequest("req-1"), PendingRequest("req-2"));
            var service = new AttendanceRequestService(repo);

            await service.AcceptAsync("req-1", "ok", "admin");

            Assert.Equal(AttendanceRequestStatus.Accepted, repo.Requests.Single(r => r.Id == "req-1").Status);
            Assert.Equal(AttendanceRequestStatus.Pending, repo.Requests.Single(r => r.Id == "req-2").Status);
        }


        public class FakeRequestRepository : IAttendanceRequestRepository
        {
            public List<AttendanceRequest> Requests { get; }
            public int UpdateCalls { get; private set; }

            public FakeRequestRepository(params AttendanceRequest[] requests) => Requests = requests.ToList();

            public Task<AttendanceRequest> CreateAsync(AttendanceRequest request)
            {
                if (string.IsNullOrEmpty(request.Id)) request.Id = Guid.NewGuid().ToString();
                Requests.Add(request);
                return Task.FromResult(request);
            }

            public Task<AttendanceRequest?> GetByIdAsync(string id) =>
                Task.FromResult(Requests.FirstOrDefault(r => r.Id == id));

            public Task<List<AttendanceRequest>> GetAllAsync() => Task.FromResult(Requests.ToList());

            public Task<List<AttendanceRequest>> GetByEmployeeIdAsync(string employeeId) =>
                Task.FromResult(Requests.Where(r => r.EmployeeId == employeeId).ToList());

            public Task<List<AttendanceRequest>> GetPendingAsync() =>
                Task.FromResult(Requests.Where(r => r.Status == AttendanceRequestStatus.Pending).ToList());

            public Task UpdateAsync(AttendanceRequest request)
            {
                UpdateCalls++;
                return Task.CompletedTask;
            }
        }
    }
}