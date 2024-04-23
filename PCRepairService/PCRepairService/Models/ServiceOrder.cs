using System.ComponentModel.DataAnnotations;

namespace PCRepairService.Models
{
    //Service Order -> simple datamodel with everything it needs
    public class ServiceOrder
    {
        public long Id { get; set; }
        public string? ServiceOrderType { get; set; }
        public string? Description { get; set; }
        public string? Name { get; set; }
        public int Cost { get; set; }
        public bool IsCompleted { get; set; }
        public DateOnly? HandoverAppointment {  get; set; }
        public DateOnly? ReturnDate { get; set; }

    }
}
