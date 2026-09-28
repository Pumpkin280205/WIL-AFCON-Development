using WIL_Afcon.Models;
using WIL_Afcon.Models.Enums;
using WIL_Afcon.Repositories.Interfaces;
using WIL_Afcon.Services.Interfaces;

namespace WIL_Afcon.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly IEmployeeRepository _employeeRepository;

        public DocumentService(
            IDocumentRepository documentRepository,
            IEmployeeRepository employeeRepository)
        {
            _documentRepository = documentRepository;
            _employeeRepository = employeeRepository;
        }

        public async Task<List<Document>> GetByEmployeeIdAsync(string employeeId)
        {
            return await _documentRepository.GetByEmployeeIdAsync(employeeId);
        }

        // ------------------------------------------------------------------
        // UPLOAD: the employee's name or employee number must appear in the
        // file name, so an admin picking the wrong employee in bulk uploads
        // gets caught before a payslip lands on the wrong record.
        // ------------------------------------------------------------------
        public async Task<Document> UploadAsync(
            string employeeId,
            DocumentType documentType,
            string fileName,
            string contentType,
            Stream fileStream)
        {
            var employee = await _employeeRepository.GetByIdAsync(employeeId);

            if (employee == null)
            {
                throw new InvalidOperationException("Employee not found.");
            }

            if (!FileNameMatchesEmployee(fileName, employee))
            {
                throw new InvalidOperationException(
                    $"Rejected: file name '{fileName}' does not appear to match employee " +
                    $"'{employee.FirstName} {employee.LastName}' ({employee.EmployeeNumber}). " +
                    "Include the employee's name or employee number in the file name.");
            }

            var document = new Document
            {
                EmployeeId = employeeId,
                DocumentType = documentType,
                FileName = fileName,
                ContentType = contentType,
                UploadedAt = DateTime.UtcNow
            };

            return await _documentRepository.UploadAsync(document, fileStream);
        }

        public async Task<(Document Document, Stream FileStream)?> GetForDownloadAsync(string documentId)
        {
            var document = await _documentRepository.GetByIdAsync(documentId);

            if (document == null)
            {
                return null;
            }

            var stream = await _documentRepository.OpenDownloadStreamAsync(document.GridFsFileId);

            return (document, stream);
        }

        private static bool FileNameMatchesEmployee(string fileName, Employee employee)
        {
            var normalizedFileName = Normalize(fileName);

            var candidates = new[]
            {
                employee.FirstName,
                employee.LastName,
                employee.EmployeeNumber
            };

            return candidates
                .Where(candidate => !string.IsNullOrWhiteSpace(candidate))
                .Select(Normalize)
                .Any(normalizedCandidate => normalizedFileName.Contains(normalizedCandidate));
        }

        private static string Normalize(string value)
        {
            return value
                .Replace(" ", "")
                .Replace("_", "")
                .Replace("-", "")
                .ToLowerInvariant();
        }
    }
}
