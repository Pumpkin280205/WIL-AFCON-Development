namespace WIL_Afcon.Models.Enums
{
    // What happened when an admin deactivated an employee, so the UI can
    // tell them exactly what was and wasn't done.
    public enum DeactivationOutcome
    {
        // No employee with that id
        NotFound,

        // Marked inactive; there was no biometric device link to remove
        Deactivated,

        // Marked inactive and removed from the biometric terminal
        DeactivatedAndRemovedFromDevice,

        // Marked inactive, but the terminal could not be reached, so the
        // person must be removed from the device manually
        DeactivatedButDeviceRemovalFailed
    }
}
