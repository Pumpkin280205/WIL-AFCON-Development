using MongoDB.Driver;
using WIL_Afcon.API.Models;

namespace WIL_Afcon.API.Data
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

        // Called once at start-up. Creating an index that already exists with the
        // same definition does nothing, so this is safe to run on every start.
        public async Task EnsureIndexesAsync(ILogger logger)
        {
            var employee = Builders<Employee>.IndexKeys;
            var employeeFilter = Builders<Employee>.Filter;

            // Unique only where a value is set: unbound employees store null or ""
            // and must not collide with each other. $gt "" matches non-empty strings.
            await CreateIndexAsync(logger, "Employees", new CreateIndexModel<Employee>(
                employee.Ascending(e => e.DeviceUserId),
                new CreateIndexOptions<Employee>
                {
                    Name = "ux_deviceUserId",
                    Unique = true,
                    PartialFilterExpression = employeeFilter.Gt(e => e.DeviceUserId, string.Empty)
                }));

            await CreateIndexAsync(logger, "Employees", new CreateIndexModel<Employee>(
                employee.Ascending(e => e.Email),
                new CreateIndexOptions<Employee>
                {
                    Name = "ux_email",
                    Unique = true,
                    PartialFilterExpression = employeeFilter.Gt(e => e.Email, string.Empty)
                }));

            await CreateIndexAsync(logger, "Employees", new CreateIndexModel<Employee>(
                employee.Ascending(e => e.EmployeeNumber),
                new CreateIndexOptions<Employee>
                {
                    Name = "ux_employeeNumber",
                    Unique = true,
                    PartialFilterExpression = employeeFilter.Gt(e => e.EmployeeNumber, string.Empty)
                }));

            // One employee record per Identity account
            await CreateIndexAsync(logger, "Employees", new CreateIndexModel<Employee>(
                employee.Ascending(e => e.UserId),
                new CreateIndexOptions<Employee>
                {
                    Name = "ux_userId",
                    Unique = true,
                    PartialFilterExpression = employeeFilter.Gt(e => e.UserId, string.Empty)
                }));

            var attendance = Builders<Attendance>.IndexKeys;

            // "Today's record for this employee" and employee history lookups.
            // Not unique: the web app and the API store AttendanceDate with
            // different times of day, so one-record-per-day cannot be enforced here.
            await CreateIndexAsync(logger, "Attendance", new CreateIndexModel<Attendance>(
                attendance.Ascending(a => a.EmployeeId).Descending(a => a.AttendanceDate),
                new CreateIndexOptions { Name = "ix_employee_date" }));

            // Admin list and reports: date-range queries sorted newest first
            await CreateIndexAsync(logger, "Attendance", new CreateIndexModel<Attendance>(
                attendance.Descending(a => a.AttendanceDate),
                new CreateIndexOptions { Name = "ix_date" }));

            var request = Builders<AttendanceRequest>.IndexKeys;

            await CreateIndexAsync(logger, "AttendanceRequests", new CreateIndexModel<AttendanceRequest>(
                request.Ascending(r => r.EmployeeId).Descending(r => r.CreatedAt),
                new CreateIndexOptions { Name = "ix_employee_created" }));

            // Admin "pending requests" queue
            await CreateIndexAsync(logger, "AttendanceRequests", new CreateIndexModel<AttendanceRequest>(
                request.Ascending(r => r.Status).Descending(r => r.CreatedAt),
                new CreateIndexOptions { Name = "ix_status_created" }));
        }

        // One index at a time so a single failure (e.g. existing duplicate data
        // blocking a unique index) does not stop the others or the API starting.
        private async Task CreateIndexAsync<T>(ILogger logger, string collectionName, CreateIndexModel<T> index)
        {
            try
            {
                await _database.GetCollection<T>(collectionName).Indexes.CreateOneAsync(index);
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "Could not create index {IndexName} on {Collection}. " +
                    "For a unique index, check the collection for duplicate values.",
                    index.Options?.Name, collectionName);
            }
        }
    }
}
