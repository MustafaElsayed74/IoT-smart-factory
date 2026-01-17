namespace IoTBackend.Models
{
    public class Reading
    {
        public int ReadingId { get; set; }
        public decimal? Temperature { get; set; }
        public bool? Smoke { get; set; }
        public decimal? Weight { get; set; }
        public string? ProductNumber { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
