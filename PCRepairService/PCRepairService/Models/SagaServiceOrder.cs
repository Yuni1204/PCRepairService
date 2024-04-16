namespace PCRepairService.Models
{
    public class SagaServiceOrder
    {
        public long Id { get; set; }
        public string? NextStep { get; set; }
        public bool? ServiceOrderCreated { get; set; }
        public bool? AppointmentDatesConfirmed { get; set; }
    }
}
