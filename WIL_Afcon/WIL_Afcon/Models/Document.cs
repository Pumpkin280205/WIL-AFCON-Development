using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using WIL_Afcon.Models.Enums;

namespace WIL_Afcon.Models
{
    public class Document
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonRepresentation(BsonType.ObjectId)]
        public string EmployeeId { get; set; } = string.Empty;

        public DocumentType DocumentType { get; set; }

        public string FileName { get; set; } = string.Empty;

        public string ContentType { get; set; } = string.Empty;

        /// <summary>Id of the actual file bytes stored in GridFS ("fs.files"/"fs.chunks").</summary>
        [BsonRepresentation(BsonType.ObjectId)]
        public string GridFsFileId { get; set; } = string.Empty;

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}
