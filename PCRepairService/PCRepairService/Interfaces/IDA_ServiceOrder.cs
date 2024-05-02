using PCRepairService.Models;

namespace PCRepairService.Interfaces
{
    public interface IDA_ServiceOrder
    {
        Task<ServiceOrder?> GetByIdAsync(long id);
        Task<IEnumerable<ServiceOrder>> GetAllAsync();
        Task AddAsync(ServiceOrder aiso);
        Task AddWithMessageAsync(ServiceOrder aiso, string exchange, string messageType);
        Task SagaAddWithMessageAsync(ServiceOrder ServiceOrder, string exchange, string messageType, string nextSaga, long sagaId);
        Task SagaMessageAsync(ServiceOrder ServiceOrder, string exchange, string messageType, string nextSaga, long sagaId, bool compensate);
        Task<long> CreateSagaAsync(String nextSaga);
        Task EditSagaAsync(long id, string nextstep, bool compensate);
        void DeleteAsync(long id);
        Task EditAsync(ServiceOrder serviceOrder);
    }
}
