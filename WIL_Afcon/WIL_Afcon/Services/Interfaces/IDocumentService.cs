using WIL_Afcon.Models;
using WIL_Afcon.Models.Enums;

namespace WIL_Afcon.Services.Interfaces
{
    public interface IDocumentService
    {
        Task<List<Document>> GetByEmployeeIdAsync(string employeeId);

        /// <summary>
        /// Validates that fileName plausibly belongs to the employee (name or
        /// employee number must appear in it) before storing it.
        /// Throws InvalidOperationException if the file is rejected.
        /// </summary>
        Task<Document> UploadAsync(
            string employeeId,
            DocumentType documentType,
            string fileName,
            string contentType,
            Stream fileStream);

        Task<(Document Document, Stream FileStream)?> GetForDownloadAsync(string documentId);
    }
}
