using MongoDB.Bson;

namespace WIL_Afcon.Models
{
    public class BreakRecord
    {
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        public DateTime StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public int DurationMinutes { get; set; }
    }
}