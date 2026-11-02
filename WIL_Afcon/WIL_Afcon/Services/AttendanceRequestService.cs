using System.Net.Http.Json;
using WIL_Afcon.Models;
using WIL_Afcon.Services.Interfaces;
using Microsoft.AspNetCore.Http;

namespace WIL_Afcon.Services
{
    public class AttendanceRequestService : IAttendanceRequestService
    {
        private readonly HttpClient _httpClient;

        public AttendanceRequestService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<AttendanceRequest>> GetAllAsync()
        {
            var response = await _httpClient.GetAsync(
                "api/attendance-requests");

            response.EnsureSuccessStatusCode();

            return await response.Content
                .ReadFromJsonAsync<List<AttendanceRequest>>()
                ?? new List<AttendanceRequest>();
        }

        public async Task<AttendanceRequest?> GetByIdAsync(string id)
        {
            var response = await _httpClient.GetAsync(
                $"api/attendance-requests/{id}");

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content
                .ReadFromJsonAsync<AttendanceRequest>();
        }

        public async Task<List<AttendanceRequest>> GetByEmployeeIdAsync(
            string employeeId)
        {
            var response = await _httpClient.GetAsync(
                $"api/attendance-requests/employee/{employeeId}");

            response.EnsureSuccessStatusCode();

            return await response.Content
                .ReadFromJsonAsync<List<AttendanceRequest>>()
                ?? new List<AttendanceRequest>();
        }

        public async Task<List<AttendanceRequest>> GetPendingAsync()
        {
            var response = await _httpClient.GetAsync(
                "api/attendance-requests/pending");

            response.EnsureSuccessStatusCode();

            return await response.Content
                .ReadFromJsonAsync<List<AttendanceRequest>>()
                ?? new List<AttendanceRequest>();
        }

        public async Task<AttendanceRequest?> CreateAsync(
     AttendanceRequest request,
     IFormFile? file)
        {
            using var form = new MultipartFormDataContent();

            form.Add(
                new StringContent(request.EmployeeId ?? string.Empty),
                "EmployeeId");

            form.Add(
                new StringContent(request.RequestType.ToString()),
                "RequestType");

            form.Add(
                new StringContent(request.RequestDate.ToString("O")),
                "RequestDate");

            if (request.StartDate.HasValue)
            {
                form.Add(
                    new StringContent(request.StartDate.Value.ToString("O")),
                    "StartDate");
            }

            if (request.EndDate.HasValue)
            {
                form.Add(
                    new StringContent(request.EndDate.Value.ToString("O")),
                    "EndDate");
            }

            if (!string.IsNullOrWhiteSpace(request.Description))
            {
                form.Add(
                    new StringContent(request.Description),
                    "Description");
            }

            if (file != null && file.Length > 0)
            {
                var fileContent = new StreamContent(
                    file.OpenReadStream());

                fileContent.Headers.ContentType =
                    new System.Net.Http.Headers.MediaTypeHeaderValue(
                        file.ContentType);

                form.Add(
                    fileContent,
                    "file",
                    file.FileName);
            }

            var response = await _httpClient.PostAsync(
                "api/attendance-requests",
                form);

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content
                .ReadFromJsonAsync<AttendanceRequest>();
        }


        public async Task<bool> AcceptAsync(
            string id,
            string? adminComment)
        {
            var response = await _httpClient.PutAsJsonAsync(
                $"api/attendance-requests/{id}/accept",
                adminComment ?? string.Empty);

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> RejectAsync(
            string id,
            string? adminComment)
        {
            var response = await _httpClient.PutAsJsonAsync(
                $"api/attendance-requests/{id}/reject",
                adminComment ?? string.Empty);

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var response = await _httpClient.DeleteAsync(
                $"api/attendance-requests/{Uri.EscapeDataString(id)}");

            return response.IsSuccessStatusCode;
        }



        public async Task<(Stream Stream, string FileName, string ContentType)?>
    DownloadDocumentAsync(string documentId)
        {
            var response = await _httpClient.GetAsync(
                $"api/attendance-documents/{documentId}");

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var stream = await response.Content.ReadAsStreamAsync();

            var fileName = "document";

            if (response.Content.Headers.ContentDisposition != null)
            {
                fileName =
                    response.Content.Headers.ContentDisposition.FileNameStar
                    ?? response.Content.Headers.ContentDisposition.FileName
                    ?? fileName;

                fileName = fileName.Trim('"');
            }

            var contentType =
                response.Content.Headers.ContentType?.MediaType
                ?? "application/octet-stream";

            return (stream, fileName, contentType);
        }


    }
}