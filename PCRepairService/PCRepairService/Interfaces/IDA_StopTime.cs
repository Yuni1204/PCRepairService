using PCRepairService.Models;

namespace PCRepairService.Interfaces
{
    public interface IDA_StopTime
    {
        Task AddNewStopTimeAsync(RepairStopTime stoptime);
    }
}
