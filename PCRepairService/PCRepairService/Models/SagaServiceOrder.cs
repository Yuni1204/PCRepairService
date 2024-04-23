namespace PCRepairService.Models
{
    public class SagaServiceOrder
    {
        public long Id { get; set; } // = serviceOrder id? würde das funktionieren?
        public string? NextStep { get; set; }
        public bool? ServiceOrderCreated { get; set; }
        public bool? AppointmentDatesConfirmed { get; set; }
        public bool? SpareCarReserved { get; set; }
    }
}
