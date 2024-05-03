namespace PCRepairService.Models
{
    public class Timestamps
    {
        public long ServiceOrderId { get; set; }
        public DateTime Timestamp1 { get; set; }
        public DateTime? Timestamp2{ get; set; }
        public TimeSpan? Duration { get; set; }
    }
}
