using AspNetCore.Identity.Mongo.Model;
using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using WIL_Afcon.Data;
using WIL_Afcon.Models;
using WIL_Afcon.Models.Enums;
using WIL_Afcon.Services.Interfaces;

namespace WIL_Afcon.Services
{
    // Cascading delete of an employee across every collection that references
    // them. Child data is removed first and the employee record last, so if a
    // step fails the employee is still listed and the delete can be retried.
    public class EmployeeDeletionService : IEmployeeDeletionService
    {
        private readonly IMongoDatabase _database;
        private readonly UserManager<MongoUser> _userManager;
        private readonly HttpClient _httpClient;

        public EmployeeDeletionService(
            MongoDbContext context,
            UserManager<MongoUser> userManager,
            HttpClient httpClient)
        {
            _database = context.Database;
            _userManager = userManager;
            _httpClient = httpClient;
        }

        public async Task<DeletionOutcome> DeleteAsync(
            string id,
            string? confirmDeviceUserId,
            string? confirmFullName)
        {
            if (!ObjectId.TryParse(id, out var objectId))
                return DeletionOutcome.NotFound;

            var employees = _database.GetCollection<Employee>("Employees");
            var employee = await employees.Find(e => e.Id == id).FirstOrDefaultAsync();

            if (employee == null)
                return DeletionOutcome.NotFound;

            if (!ConfirmationMatches(employee, confirmDeviceUserId, confirmFullName))
                return DeletionOutcome.ConfirmationMismatch;

            // EmployeeId may be stored as an ObjectId or a plain string depending on who wrote it
            var byEmployee = Builders<BsonDocument>.Filter.In(
                "EmployeeId", new BsonValue[] { objectId, new BsonString(id) });

            // Attendance requests and their supporting files (API's default GridFS bucket)
            var requests = _database.GetCollection<BsonDocument>("AttendanceRequests");
            var requestFiles = (await requests.Find(byEmployee).ToListAsync())
                .Select(r => r.GetValue("DocumentId", BsonNull.Value))
                .Where(v => v.IsString)
                .Select(v => v.AsString);

            await DeleteFilesAsync(new GridFSBucket(_database), requestFiles);
            await requests.DeleteManyAsync(byEmployee);

            // Payslips / IRP5s and their files
            var documents = _database.GetCollection<BsonDocument>("Documents");
            var documentFiles = (await documents.Find(byEmployee).ToListAsync())
                .Select(d => d.GetValue("GridFsFileId", BsonNull.Value))
                .Where(v => !v.IsBsonNull)
                .Select(v => v.ToString()!);

            await DeleteFilesAsync(
                new GridFSBucket(_database, new GridFSBucketOptions { BucketName = "DocumentFiles" }),
                documentFiles);
            await documents.DeleteManyAsync(byEmployee);

            await _database.GetCollection<BsonDocument>("Attendance").DeleteManyAsync(byEmployee);

            // Their login account, so they can no longer sign in
            if (!string.IsNullOrWhiteSpace(employee.UserId))
            {
                var user = await _userManager.FindByIdAsync(employee.UserId);

                if (user != null)
                {
                    var result = await _userManager.DeleteAsync(user);

                    if (!result.Succeeded)
                        throw new InvalidOperationException(
                            "Could not delete the employee's login account: " +
                            string.Join(" ", result.Errors.Select(e => e.Description)));
                }
            }

            await employees.DeleteOneAsync(e => e.Id == id);

            return await RemoveFromDeviceAsync(employee.DeviceUserId);
        }

        // Device User ID is the primary check; employees never linked to a device
        // confirm with their Employee Number instead. Names ignore case and extra spaces.
        public static bool ConfirmationMatches(
            Employee employee,
            string? confirmDeviceUserId,
            string? confirmFullName)
        {
            var expectedId = string.IsNullOrWhiteSpace(employee.DeviceUserId)
                ? employee.EmployeeNumber
                : employee.DeviceUserId;

            if (string.IsNullOrWhiteSpace(expectedId) ||
                !string.Equals(confirmDeviceUserId?.Trim(), expectedId.Trim(), StringComparison.Ordinal))
                return false;

            return string.Equals(
                NormaliseName(confirmFullName),
                NormaliseName($"{employee.FirstName} {employee.LastName}"),
                StringComparison.OrdinalIgnoreCase);
        }

        private static string NormaliseName(string? name) =>
            string.Join(' ', (name ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries));

        private static async Task DeleteFilesAsync(GridFSBucket bucket, IEnumerable<string> fileIds)
        {
            foreach (var fileId in fileIds)
            {
                if (!ObjectId.TryParse(fileId, out var fileObjectId))
                    continue;

                try
                {
                    await bucket.DeleteAsync(fileObjectId);
                }
                catch (GridFSFileNotFoundException)
                {
                    // Already gone - nothing to clean up
                }
            }
        }

        // Best effort, as with deactivation: the employee record is already gone,
        // so any further scans are rejected by the API even if this fails.
        private async Task<DeletionOutcome> RemoveFromDeviceAsync(string? deviceUserId)
        {
            if (string.IsNullOrWhiteSpace(deviceUserId))
                return DeletionOutcome.Deleted;

            try
            {
                var response = await _httpClient.DeleteAsync(
                    $"api/attendance/device/employee/{Uri.EscapeDataString(deviceUserId)}");

                return response.IsSuccessStatusCode
                    ? DeletionOutcome.Deleted
                    : DeletionOutcome.DeletedButDeviceRemovalFailed;
            }
            catch (HttpRequestException)
            {
                return DeletionOutcome.DeletedButDeviceRemovalFailed;
            }
        }
    }
}
