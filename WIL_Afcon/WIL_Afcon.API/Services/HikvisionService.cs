using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using WIL_Afcon.API.Configuration;
using WIL_Afcon.API.Services.Interfaces;

namespace WIL_Afcon.API.Services
{
    public class HikvisionService : IHikvisionService
    {
        private readonly HikvisionDeviceOptions _options;
        private readonly ILogger<HikvisionService> _logger;

        public HikvisionService(
            IOptions<HikvisionDeviceOptions> options,
            ILogger<HikvisionService> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public async Task<bool> DeleteEmployeeAsync(string deviceUserId)
        {
            if (string.IsNullOrWhiteSpace(deviceUserId))
            {
                return false;
            }

            var scheme = _options.UseHttps ? "https" : "http";

            var baseUrl =
                $"{scheme}://{_options.Host}:{_options.Port}";

            var deleteUrl =
                $"{baseUrl}/ISAPI/AccessControl/UserInfoDetail/Delete?format=json";

            var requestBody = new
            {
                UserInfoDetail = new
                {
                    mode = "all",
                    EmployeeNoList = new[]
                    {
                new
                {
                    employeeNo = deviceUserId
                }
            }
                }
            };

            var json = JsonSerializer.Serialize(requestBody);

            using var handler = new HttpClientHandler
            {
                Credentials = new NetworkCredential(
                    _options.Username,
                    _options.Password)
            };

            using var client = new HttpClient(handler);

            try
            {
                // Step 1: Start the deletion
                using var deleteRequest = new HttpRequestMessage(
                    HttpMethod.Put,
                    deleteUrl);

                deleteRequest.Content = new StringContent(
                    json,
                    Encoding.UTF8,
                    "application/json");

                var deleteResponse = await client.SendAsync(deleteRequest);

                var deleteResponseBody =
                    await deleteResponse.Content.ReadAsStringAsync();

                _logger.LogInformation(
                    "Hikvision delete response for device user {DeviceUserId}: {StatusCode} {Response}",
                    deviceUserId,
                    deleteResponse.StatusCode,
                    deleteResponseBody);

                if (!deleteResponse.IsSuccessStatusCode)
                {
                    return false;
                }

                // Step 2: Wait for Hikvision to actually finish deleting
                var processUrl =
                    $"{baseUrl}/ISAPI/AccessControl/UserInfoDetail/DeleteProcess?format=json";

                for (var attempt = 0; attempt < 20; attempt++)
                {
                    await Task.Delay(500);

                    using var processRequest = new HttpRequestMessage(
                        HttpMethod.Get,
                        processUrl);

                    var processResponse = await client.SendAsync(processRequest);

                    var processResponseBody =
                        await processResponse.Content.ReadAsStringAsync();

                    _logger.LogInformation(
                        "Hikvision delete process for device user {DeviceUserId}: {StatusCode} {Response}",
                        deviceUserId,
                        processResponse.StatusCode,
                        processResponseBody);

                    if (!processResponse.IsSuccessStatusCode)
                    {
                        return false;
                    }

                    try
                    {
                        using var document =
                            JsonDocument.Parse(processResponseBody);

                        if (document.RootElement.TryGetProperty(
                                "UserInfoDetailDeleteProcess",
                                out var process))
                        {
                            var status = process
                                .GetProperty("status")
                                .GetString();

                            if (string.Equals(
                                    status,
                                    "success",
                                    StringComparison.OrdinalIgnoreCase))
                            {
                                return true;
                            }

                            if (!string.Equals(
                                    status,
                                    "processing",
                                    StringComparison.OrdinalIgnoreCase))
                            {
                                return false;
                            }
                        }
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogError(
                            ex,
                            "Could not parse Hikvision delete process response for device user {DeviceUserId}.",
                            deviceUserId);

                        return false;
                    }
                }

                _logger.LogWarning(
                    "Hikvision deletion timed out for device user {DeviceUserId}.",
                    deviceUserId);

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to delete device user {DeviceUserId} from Hikvision.",
                    deviceUserId);

                return false;
            }
        }
    }
}