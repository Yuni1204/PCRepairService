using PCRepairService.Models;

namespace PCRepairService.Interfaces
{
    public interface IDA_ServiceOrder
    {
        Task<ServiceOrder?> GetByIdAsync(long id);
        Task<IEnumerable<ServiceOrder>> GetAllAsync();
        Task AddAsync(ServiceOrder aiso);
        Task AddWithMessageAsync(ServiceOrder aiso, string exchange, string messageType);
        void DeleteAsync(long id);
    }
}
