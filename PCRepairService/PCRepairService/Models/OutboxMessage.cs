namespace PCRepairService.Models
{
    public class OutboxMessage
    {
        //not used
        public long Id { get; set; }
        public string? exchange {  get; set; }
        public string? messageType { get; set; }
        public string? content { get; set; }
        public DateTime Timestamp { get; set; }

    }
}
