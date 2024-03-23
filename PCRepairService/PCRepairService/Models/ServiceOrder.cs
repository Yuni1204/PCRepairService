using System.ComponentModel.DataAnnotations.Schema;

namespace PCRepairService.Models
{
    public class ServiceOrder
    {
        public long Id { get; set; }
        public ServiceOrderType ServiceOrderType { get; set; }
        public string Description { get; set; }
        public long KundeId { get; set; }
        [ForeignKey("KundeId")]
        public Kunde Kunde { get; set; }
    }
}
