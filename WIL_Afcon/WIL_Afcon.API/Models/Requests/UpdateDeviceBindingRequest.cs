namespace WIL_Afcon.API.Models.Requests
{
    /// <summary>
    /// Body for PUT /api/employees/{id}/device-binding.
    /// Send the terminal's "Employee No." to bind it, or null/empty to unbind.
    /// </summary>
    public class UpdateDeviceBindingRequest
    {
        public string? DeviceUserId { get; set; }
    }
}
