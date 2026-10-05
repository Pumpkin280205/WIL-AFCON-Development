using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WIL_Afcon.Controllers;
using WIL_Afcon.Models;
using WIL_Afcon.Models.Enums;
using WIL_Afcon.Repositories.Interfaces;
using WIL_Afcon.Services;
using WIL_Afcon.Services.Interfaces;
using Xunit;

namespace WIL_Afcon.Tests
{
    // Document rules (WIL_Afcon web app)
    //  - Services/DocumentService.cs: file name must contain the employee's name or number
    //  - Controllers/DocumentController.cs: an employee can't download someone else's document
    public class DocumentTests
    {
        private static Employee Jane() => new Employee
        {
            Id = "emp-jane",
            FirstName = "Jane",
            LastName = "Doe",
            EmployeeNumber = "EMP001",
            UserId = "user-jane"
        };

        private static Employee John() => new Employee
        {
            Id = "emp-john",
            FirstName = "John",
            LastName = "Smith",
            EmployeeNumber = "EMP002",
            UserId = "user-john"
        };

        private static Stream AnyFile() => new MemoryStream(Encoding.UTF8.GetBytes("pdf bytes"));

        // =====================================================================
        // UPLOAD: file name must match the employee
        // =====================================================================

        [Theory]
        [InlineData("Payslip_Jane_March.pdf")]      // first name
        [InlineData("doe-irp5-2026.pdf")]           // last name, lower case
        [InlineData("EMP001_payslip.pdf")]          // employee number
        [InlineData("emp-001 payslip.pdf")]         // number with dash/space
        public async Task Upload_FileNameContainsNameOrNumber_IsAccepted(string fileName)
        {
            var docs = new FakeDocumentRepository();
            var service = new DocumentService(docs, new FakeEmployeeRepository(Jane()));

            var saved = await service.UploadAsync(
                "emp-jane", DocumentType.Payslip, fileName, "application/pdf", AnyFile());

            Assert.Equal("emp-jane", saved.EmployeeId);
            Assert.Single(docs.Documents);
        }

        [Theory]
        [InlineData("Payslip_John_Smith_March.pdf")] // someone else's name
        [InlineData("EMP002_payslip.pdf")]          // someone else's number
        [InlineData("payslip.pdf")]                 // no name at all
        public async Task Upload_FileNameDoesNotMatchEmployee_IsRejected(string fileName)
        {
            var docs = new FakeDocumentRepository();
            var service = new DocumentService(docs, new FakeEmployeeRepository(Jane()));

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.UploadAsync("emp-jane", DocumentType.Payslip, fileName, "application/pdf", AnyFile()));

            Assert.Empty(docs.Documents); // nothing was stored
        }

        [Fact]
        public async Task Upload_ForUnknownEmployee_IsRejected()
        {
            var docs = new FakeDocumentRepository();
            var service = new DocumentService(docs, new FakeEmployeeRepository());

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.UploadAsync("does-not-exist", DocumentType.IRP5, "anything.pdf", "application/pdf", AnyFile()));

            Assert.Empty(docs.Documents);
        }

        // =====================================================================
        // DOWNLOAD: employees may only download their own documents
        // =====================================================================

        private static DocumentController CreateController(ClaimsPrincipal user)
        {
            var janesDocument = new Document
            {
                Id = "doc-1",
                EmployeeId = "emp-jane",
                FileName = "Payslip_Jane.pdf",
                ContentType = "application/pdf"
            };

            var controller = new DocumentController(
                new FakeDocumentService(janesDocument),
                new FakeEmployeeService(Jane(), John()));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            return controller;
        }

        private static ClaimsPrincipal LoggedInAs(string userId, string role)
        {
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Role, role)
            }, "TestAuth");

            return new ClaimsPrincipal(identity);
        }

        [Fact]
        public async Task Download_EmployeeOwnDocument_ReturnsFile()
        {
            var controller = CreateController(LoggedInAs("user-jane", "Employee"));

            var result = await controller.Download("doc-1");

            Assert.IsType<FileStreamResult>(result);
        }

        [Fact]
        public async Task Download_SomeoneElsesDocument_IsForbidden()
        {
            var controller = CreateController(LoggedInAs("user-john", "Employee"));

            var result = await controller.Download("doc-1");

            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task Download_AdminCanDownloadAnyDocument()
        {
            var controller = CreateController(LoggedInAs("user-admin", "Admin"));

            var result = await controller.Download("doc-1");

            Assert.IsType<FileStreamResult>(result);
        }

        [Fact]
        public async Task Download_MissingDocument_ReturnsNotFound()
        {
            var controller = CreateController(LoggedInAs("user-jane", "Employee"));

            var result = await controller.Download("no-such-doc");

            Assert.IsType<NotFoundResult>(result);
        }

        // =====================================================================
        // Fakes (in memory, no MongoDB)
        // =====================================================================

        public class FakeEmployeeRepository : IEmployeeRepository
        {
            private readonly List<Employee> _employees;

            public FakeEmployeeRepository(params Employee[] employees) => _employees = employees.ToList();

            public Task<Employee?> GetByIdAsync(string id) =>
                Task.FromResult(_employees.FirstOrDefault(e => e.Id == id));

            public Task<List<Employee>> GetAllAsync() => Task.FromResult(_employees.ToList());
            public Task CreateAsync(Employee employee) { _employees.Add(employee); return Task.CompletedTask; }
            public Task UpdateAsync(Employee employee) => Task.CompletedTask;
            public Task DeleteAsync(string id) => Task.CompletedTask;
            public Task<Employee?> GetByUserIdAsync(string userId) =>
                Task.FromResult(_employees.FirstOrDefault(e => e.UserId == userId));
            public Task<Employee?> GetByEmailAsync(string email) =>
                Task.FromResult(_employees.FirstOrDefault(e => e.Email == email));
            public Task<Employee?> GetByDeviceUserIdAsync(string deviceUserId) =>
                Task.FromResult(_employees.FirstOrDefault(e => e.DeviceUserId == deviceUserId));
        }

        public class FakeDocumentRepository : IDocumentRepository
        {
            public List<Document> Documents { get; } = new();

            public Task<Document> UploadAsync(Document document, Stream fileStream)
            {
                document.Id = Guid.NewGuid().ToString();
                Documents.Add(document);
                return Task.FromResult(document);
            }

            public Task<List<Document>> GetByEmployeeIdAsync(string employeeId) =>
                Task.FromResult(Documents.Where(d => d.EmployeeId == employeeId).ToList());

            public Task<Document?> GetByIdAsync(string id) =>
                Task.FromResult(Documents.FirstOrDefault(d => d.Id == id));

            public Task<Stream> OpenDownloadStreamAsync(string gridFsFileId) =>
                Task.FromResult<Stream>(new MemoryStream());
        }

        public class FakeDocumentService : IDocumentService
        {
            private readonly Document _document;

            public FakeDocumentService(Document document) => _document = document;

            public Task<(Document Document, Stream FileStream)?> GetForDownloadAsync(string documentId)
            {
                if (documentId != _document.Id)
                {
                    return Task.FromResult<(Document, Stream)?>(null);
                }

                return Task.FromResult<(Document, Stream)?>((_document, new MemoryStream()));
            }

            public Task<List<Document>> GetByEmployeeIdAsync(string employeeId) =>
                Task.FromResult(new List<Document>());

            public Task<Document> UploadAsync(string employeeId, DocumentType documentType,
                string fileName, string contentType, Stream fileStream) =>
                throw new NotImplementedException();
        }

        public class FakeEmployeeService : IEmployeeService
        {
            private readonly List<Employee> _employees;

            public FakeEmployeeService(params Employee[] employees) => _employees = employees.ToList();

            public Task<Employee?> GetByUserIdAsync(string userId) =>
                Task.FromResult(_employees.FirstOrDefault(e => e.UserId == userId));

            public Task<Employee?> GetByIdAsync(string id) =>
                Task.FromResult(_employees.FirstOrDefault(e => e.Id == id));

            public Task<List<Employee>> GetAllAsync() => Task.FromResult(_employees.ToList());
            public Task CreateAsync(Employee employee) => Task.CompletedTask;
            public Task UpdateAsync(Employee employee) => Task.CompletedTask;
            public Task<DeactivationOutcome> DeactivateAsync(string id) =>
                Task.FromResult(DeactivationOutcome.Deactivated);
            public Task<Employee?> GetByEmailAsync(string email) =>
                Task.FromResult(_employees.FirstOrDefault(e => e.Email == email));
        }
    }
}