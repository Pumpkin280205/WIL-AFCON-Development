using WIL_Afcon.Models;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace WIL_Afcon.Services.Interfaces
{
    public interface IAttendanceRequestService
    {
        Task<List<AttendanceRequest>> GetAllAsync();

        Task<AttendanceRequest?> GetByIdAsync(string id);

        Task<List<AttendanceRequest>> GetByEmployeeIdAsync(
            string employeeId);

        Task<List<AttendanceRequest>> GetPendingAsync();

        Task<AttendanceRequest?> CreateAsync(
     AttendanceRequest request,
     IFormFile? file);

        Task<bool> AcceptAsync(
            string id,
            string? adminComment);

        Task<bool> RejectAsync(
            string id,
            string? adminComment);


        Task<(Stream Stream, string FileName, string ContentType)?>
    DownloadDocumentAsync(string documentId);
    }
}