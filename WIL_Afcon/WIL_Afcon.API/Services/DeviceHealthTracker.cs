using System.Collections.Concurrent;

namespace WIL_Afcon.API.Services
{
    /// <summary>
    /// Process-wide, in-memory record of what the biometric terminal has sent us.
    /// Registered as a singleton so the de-dupe cache and "last seen" timestamps
    /// survive across requests. Not persisted - resets when the API restarts.
    /// </summary>
    public sealed class DeviceHealthTracker
    {
        private readonly ConcurrentDictionary<string, DateTime> _recentPunches = new();
        private long _totalEvents;
        private long _totalPunches;

        public DateTime? LastEventUtc { get; private set; }

        public DateTime? LastPunchAppliedUtc { get; private set; }

        public long TotalEventsReceived => Interlocked.Read(ref _totalEvents);

        public long TotalPunchesApplied => Interlocked.Read(ref _totalPunches);

        public void MarkEventReceived()
        {
            LastEventUtc = DateTime.UtcNow;
            Interlocked.Increment(ref _totalEvents);
        }

        public void MarkPunchApplied()
        {
            LastPunchAppliedUtc = DateTime.UtcNow;
            Interlocked.Increment(ref _totalPunches);
        }

        /// <summary>
        /// Returns true if <paramref name="key"/> was already seen within
        /// <paramref name="window"/>. Terminals fire the same punch several times and
        /// re-POST until they get HTTP 200, so callers should drop the repeats.
        /// </summary>
        public bool IsDuplicate(string key, TimeSpan window)
        {
            var now = DateTime.UtcNow;

            var found = _recentPunches.TryGetValue(key, out var last);

            var isDuplicate = found && now - last < window;

            _recentPunches[key] = now;

            foreach (var entry in _recentPunches)
            {
                if (now - entry.Value > TimeSpan.FromMinutes(10))
                {
                    _recentPunches.TryRemove(entry.Key, out _);
                }
            }

            return isDuplicate;
        }
    }
}
