using WIL_Afcon.Models.Enums;

namespace WIL_Afcon.Services.Interfaces
{
    public interface IEmployeeDeletionService
    {
        // Permanently deletes the employee and everything linked to them, but only
        // when the typed Device User ID (or Employee Number if no device is linked)
        // and full name both match the stored employee.
        Task<DeletionOutcome> DeleteAsync(string id, string? confirmDeviceUserId, string? confirmFullName);
    }
}
