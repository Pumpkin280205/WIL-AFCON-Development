namespace WIL_Afcon.API.Services
{
    /// <summary>
    /// The attendance step a terminal scan should trigger, derived from the
    /// terminal's <c>attendanceStatus</c> field (or inferred when it is empty).
    /// </summary>
    public enum PunchAction
    {
        Infer,
        ClockIn,
        ClockOut,
        StartBreak,
        EndBreak,
    }
}
