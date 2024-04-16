using PCRepairService.Models;

namespace PCRepairService.Interfaces
{
    public interface IDA_ServiceOrder
    {
        Task<ServiceOrder?> GetByIdAsync(long id);
        Task<IEnumerable<ServiceOrder>> GetAllAsync();
        Task AddAsync(ServiceOrder aiso);
        Task AddWithMessageAsync(ServiceOrder aiso, string exchange, string messageType, bool isSaga = false);
        void DeleteAsync(long id);
        Task EditAsync(long id);
        Task EditSagaAsync(long id);
    }
}
