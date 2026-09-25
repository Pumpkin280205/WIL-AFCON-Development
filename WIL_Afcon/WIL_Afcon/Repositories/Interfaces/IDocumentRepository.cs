using WIL_Afcon.Models;

namespace WIL_Afcon.Repositories.Interfaces
{
    public interface IDocumentRepository
    {
        Task<List<Document>> GetByEmployeeIdAsync(string employeeId);

        Task<Document?> GetByIdAsync(string id);

        /// <summary>Stores the file bytes in GridFS and the metadata record, returning the saved Document.</summary>
        Task<Document> UploadAsync(Document document, Stream fileStream);

        /// <summary>Reads the file bytes for a document back out of GridFS.</summary>
        Task<Stream> OpenDownloadStreamAsync(string gridFsFileId);
    }
}
