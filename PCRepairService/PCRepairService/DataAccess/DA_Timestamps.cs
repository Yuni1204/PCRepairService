using PCRepairService.Models;
using PCRepairService.Interfaces;

namespace PCRepairService.DataAccess
{
    public class DA_Timestamps : IDA_Timestamps
    {
        private readonly ServiceDBContext _context;

        public DA_Timestamps(ServiceDBContext context)
        {
            _context = context;
        }

        public async Task AddTimeSpanAsync(Timestamps timestamp)
        {
            await _context.Timestamps.AddAsync(timestamp);
            await _context.SaveChangesAsync();
        }
    }
}
