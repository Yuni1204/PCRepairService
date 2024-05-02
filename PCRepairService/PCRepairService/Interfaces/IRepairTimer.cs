using PCRepairService.Models;

namespace PCRepairService.Interfaces
{
    public interface IRepairTimer
    {
        void AddStoppedTime(RepairStopTime stoppedtime);
        Task SaveStoppedTime(long id);
    }
}
