namespace WIL_Afcon.Models.Enums
{
    // What happened when an admin permanently deleted an employee, so the UI
    // can tell them exactly what was and wasn't done.
    public enum DeletionOutcome
    {
        // No employee with that id
        NotFound,

        // The typed Device User ID / name did not match - nothing was deleted
        ConfirmationMismatch,

        // Employee and all their data deleted (and removed from the device, if linked)
        Deleted,

        // Everything deleted, but the terminal could not be reached, so the
        // person must be removed from the device manually
        DeletedButDeviceRemovalFailed
    }
}
