namespace PCRepairService.Models
{
    public class RepairStopTime
    {
        public long ServiceOrderId {  get; set; }
        public int StopTime { get; set; }
        public string? Type { get; set; }
    }
}
