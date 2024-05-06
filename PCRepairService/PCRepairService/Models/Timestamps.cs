namespace PCRepairService.Models
{
    public class Timestamps
    {
        public long ServiceOrderId { get; set; }
        public DateTime Timestamp1 { get; set; }
        public DateTime? Timestamp2{ get; set; }
        public double? Duration { get; set; }
        public string? Type { get; set; }
    }
}
