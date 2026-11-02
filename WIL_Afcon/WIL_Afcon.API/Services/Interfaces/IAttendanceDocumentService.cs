namespace WIL_Afcon.API.Services.Interfaces
{
    public interface IAttendanceDocumentService
    {
        Task<string> UploadAsync(
            Stream fileStream,
            string fileName,
            string contentType);

        Task<(Stream Stream, string FileName, string ContentType)?>
            DownloadAsync(string documentId);

        Task DeleteAsync(string documentId);
    }
}