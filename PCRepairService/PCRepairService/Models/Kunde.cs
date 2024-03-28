using System.Numerics;

namespace PCRepairService.Models
{
    public class Kunde
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public long credit {  get; set; }

        //public ICollection<ServiceOrder>? ServiceOrder { get; set; }
    }
}
