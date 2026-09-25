using WIL_Afcon.Models;

namespace WIL_Afcon.Repositories.Interfaces
{
    public interface IDocumentRepository
    {
        Task<List<Document>> GetByEmployeeIdAsync(string employeeId);

        Task<Document?> GetByIdAsync(string id);

        //Stores the file bytes in GridFS and the metadata record, returning the saved Document.
        Task<Document> UploadAsync(Document document, Stream fileStream);

        //Reads the file bytes for a document back out of GridFS.
        Task<Stream> OpenDownloadStreamAsync(string gridFsFileId);
    }
}
