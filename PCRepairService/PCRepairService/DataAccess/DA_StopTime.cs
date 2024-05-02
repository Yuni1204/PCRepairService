using PCRepairService.Interfaces;
using PCRepairService.Models;

namespace PCRepairService.DataAccess
{
    public class DA_StopTime : IDA_StopTime
    {
        private readonly ServiceDBContext _context;

        public DA_StopTime(ServiceDBContext context)
        {
            _context = context;
        }

        public async Task AddNewStopTimeAsync(RepairStopTime stoptime)
        {
            await _context.StopTime.AddAsync(stoptime);
            await _context.SaveChangesAsync();
        }
    }
}
