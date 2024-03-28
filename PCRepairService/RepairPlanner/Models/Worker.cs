namespace RepairPlanner.Models
{
    public class Worker
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public bool IsBusy { get; set; }
        public long CurrentSOId { get; set; }


    }
}
