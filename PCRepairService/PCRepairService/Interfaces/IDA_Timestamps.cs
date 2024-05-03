using NuGet.Packaging.Signing;
using PCRepairService.Models;

namespace PCRepairService.Interfaces
{
    public interface IDA_Timestamps
    {
        Task AddTimeSpanAsync(Timestamps timestamp);
    }
}
