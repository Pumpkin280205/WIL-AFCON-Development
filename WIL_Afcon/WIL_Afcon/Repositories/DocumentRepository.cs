using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using WIL_Afcon.Data;
using WIL_Afcon.Models;
using WIL_Afcon.Repositories.Interfaces;

namespace WIL_Afcon.Repositories
{
    public class DocumentRepository : IDocumentRepository
    {
        private readonly IMongoCollection<Document> _documents;
        private readonly GridFSBucket _fileBucket;

        public DocumentRepository(MongoDbContext context)
        {
            _documents = context.Database.GetCollection<Document>("Documents");
            _fileBucket = new GridFSBucket(context.Database, new GridFSBucketOptions
            {
                BucketName = "DocumentFiles"
            });
        }

        public async Task<List<Document>> GetByEmployeeIdAsync(string employeeId)
        {
            return await _documents
                .Find(d => d.EmployeeId == employeeId)
                .SortByDescending(d => d.UploadedAt)
                .ToListAsync();
        }

        public async Task<Document?> GetByIdAsync(string id)
        {
            return await _documents.Find(d => d.Id == id).FirstOrDefaultAsync();
        }

        public async Task<Document> UploadAsync(Document document, Stream fileStream)
        {
            var gridFsId = await _fileBucket.UploadFromStreamAsync(document.FileName, fileStream);
            document.GridFsFileId = gridFsId.ToString();

            await _documents.InsertOneAsync(document);

            return document;
        }

        public async Task<Stream> OpenDownloadStreamAsync(string gridFsFileId)
        {
            return await _fileBucket.OpenDownloadStreamAsync(ObjectId.Parse(gridFsFileId));
        }
    }
}
