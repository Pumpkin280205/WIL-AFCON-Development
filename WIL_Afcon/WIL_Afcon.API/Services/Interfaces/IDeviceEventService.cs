using WIL_Afcon.API.Models.Devices;

namespace WIL_Afcon.API.Services.Interfaces
{
    public interface IDeviceEventService
    {
        /// <summary>
        /// Processes one access event pushed by the Hikvision terminal: de-dupes it,
        /// maps the device user to an employee and applies the matching attendance
        /// action. Pass null for an empty keep-alive POST. Returns a short,
        /// log-friendly description of the outcome; never throws for expected cases.
        /// </summary>
        Task<string> HandleHikvisionEventAsync(AccessControllerEvent? deviceEvent);

        DeviceHealthResponse GetHealth();
    }
}
