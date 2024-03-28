using PCRepairService.Models;

namespace PCRepairService.Interfaces
{
    public interface IDA_AISO
    {
        Task<AISO> GetByIdAsync(long id);
        Task<IEnumerable<AISO>> GetAllAsync();
        Task AddAsync(AISO aiso);
        void DeleteAsync(long id);
    }
}
