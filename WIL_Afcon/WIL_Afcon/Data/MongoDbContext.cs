using MongoDB.Driver;

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
