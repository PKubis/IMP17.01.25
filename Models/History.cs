using IMP.Models;

namespace IMP.Models
{
    public class ScheduledHistoryEntry
    {
        public string Id { get; set; }
        public string SectionName { get; set; }
        public string Date { get; set; }
        public int Duration { get; set; }
        public double WaterUsageLiters { get; set; }
        public double WaterUsageCubicMeters { get; set; }
    }

    public class ManualHistoryEntry
    {
        public string Id { get; set; }
        public string SectionName { get; set; }
        public string Date { get; set; }
        public int Duration { get; set; }
        public double WaterUsageLiters { get; set; }
        public double WaterUsageCubicMeters { get; set; }
    }
}
