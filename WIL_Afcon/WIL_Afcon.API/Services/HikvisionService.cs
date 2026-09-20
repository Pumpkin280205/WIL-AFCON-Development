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

            var url =
                $"{scheme}://{_options.Host}:{_options.Port}" +
                "/ISAPI/AccessControl/UserInfoDetail/Delete?format=json";

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

            using var request = new HttpRequestMessage(
                HttpMethod.Put,
                url);

            request.Content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json");

            try
            {
                var response = await client.SendAsync(request);

                var responseBody = await response.Content.ReadAsStringAsync();

                _logger.LogInformation(
                    "Hikvision delete response for device user {DeviceUserId}: {StatusCode} {Response}",
                    deviceUserId,
                    response.StatusCode,
                    responseBody);

                if (!response.IsSuccessStatusCode)
                {
                    return false;
                }

                return true;
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