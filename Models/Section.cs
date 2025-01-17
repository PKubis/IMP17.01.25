namespace IMP.Models
{
    public class Section
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string StartTime { get; set; }
        public int Duration { get; set; }
        public string SelectedDays { get; set; }
        public string WateringType { get; set; }
        public string Status { get; set; } = "stop";
        public int ElapsedTime { get; set; } // Pole ElapsedTime
        
    }
}
