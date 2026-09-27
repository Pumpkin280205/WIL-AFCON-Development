using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using WIL_Afcon.API.Data;
using WIL_Afcon.API.Services.Interfaces;

namespace WIL_Afcon.API.Services
{
    public class AttendanceDocumentService : IAttendanceDocumentService
    {
        private readonly GridFSBucket _gridFsBucket;

        public AttendanceDocumentService(MongoDbContext context)
        {
            _gridFsBucket = new GridFSBucket(context.Database);
        }

        public async Task<string> UploadAsync(
            Stream fileStream,
            string fileName,
            string contentType)
        {
            var options = new GridFSUploadOptions
            {
                Metadata = new MongoDB.Bson.BsonDocument
                {
                    { "contentType", contentType }
                }
            };

            var fileId = await _gridFsBucket.UploadFromStreamAsync(
                fileName,
                fileStream,
                options);

            return fileId.ToString();
        }

        public async Task<(Stream Stream, string FileName, string ContentType)?>
            DownloadAsync(string documentId)
        {
            if (!MongoDB.Bson.ObjectId.TryParse(
                    documentId,
                    out var objectId))
            {
                return null;
            }

            var filter = Builders<GridFSFileInfo>.Filter.Eq(
                x => x.Id,
                objectId);

            var fileInfo = await _gridFsBucket
                .Find(filter)
                .FirstOrDefaultAsync();

            if (fileInfo == null)
            {
                return null;
            }

            var stream = await _gridFsBucket
                .OpenDownloadStreamAsync(objectId);

            var contentType = "application/octet-stream";

            if (fileInfo.Metadata != null &&
                fileInfo.Metadata.Contains("contentType"))
            {
                contentType =
                    fileInfo.Metadata["contentType"].AsString;
            }

            return (
                stream,
                fileInfo.Filename,
                contentType
            );
        }
    }
}