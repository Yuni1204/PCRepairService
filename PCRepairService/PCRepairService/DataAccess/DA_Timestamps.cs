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

        public async Task AddAsync(string timestampstr, long id)
        {
            string[] splitTimestamp = timestampstr.Split('.');
            var entry = new Timestamps
            {
                SagaNumber = id,
                minute = Int32.Parse(splitTimestamp[1]),
                second = Int32.Parse(splitTimestamp[2]),
                subsecond = Int32.Parse(splitTimestamp[3]),
            };
            await _context.Timestamps.AddAsync(entry);
            await _context.SaveChangesAsync();
        }
    }
}
