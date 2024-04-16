using PCRepairService.Models;

namespace PCRepairService.Interfaces
{
    public interface ISagaHandler
    {
        Task StartServiceOrderSagaAsync(ServiceOrder serviceOrder);
        Task EndServiceOrderSagaAsync(long id);
    }
}
