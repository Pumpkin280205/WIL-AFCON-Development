using MongoDB.Driver;
using WIL_Afcon.Models;

namespace WIL_Afcon.Data
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(IConfiguration configuration)
        {
            var connectionString =
                configuration["MongoDbSettings:ConnectionString"];

            var databaseName =
                configuration["MongoDbSettings:DatabaseName"];

            var client = new MongoClient(connectionString);

            _database = client.GetDatabase(databaseName);
        }

        public IMongoDatabase Database => _database;

        // Creates indexes at start-up for web-only collections (the API indexes the rest)
        public async Task EnsureIndexesAsync(ILogger logger)
        {
            // An employee's documents, newest first
            var index = new CreateIndexModel<Document>(
                Builders<Document>.IndexKeys
                    .Ascending(d => d.EmployeeId)
                    .Descending(d => d.UploadedAt),
                new CreateIndexOptions { Name = "ix_employee_uploaded" });

            try
            {
                await _database.GetCollection<Document>("Documents").Indexes.CreateOneAsync(index);
            }
            catch (Exception ex)
            {
                // Log and keep starting
                logger.LogError(ex, "Could not create index {IndexName} on Documents.", index.Options.Name);
            }
        }
    }
}


// Microsoft (2026) 'IConfiguration Interface', Microsoft Learn.

// Available at: https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.configuration.iconfiguration

// (Accessed: 26 September 2026).

//

// MongoDB (2026) 'IMongoClient.GetDatabase Method',

// MongoDB C# Driver API Documentation.

// Available at: https://mongodb.github.io/mongo-csharp-driver/3.5.0/api/MongoDB.Driver/MongoDB.Driver.IMongoClient.GetDatabase.html

// (Accessed: 26 September 2026).

//

// MongoDB (2026) 'MongoClient Class',

// MongoDB C# Driver API Documentation.

// Available at: https://mongodb.github.io/mongo-csharp-driver/3.5.0/api/MongoDB.Driver/MongoClient.html

// (Accessed: 26 September 2026).

//

// ============================================================
