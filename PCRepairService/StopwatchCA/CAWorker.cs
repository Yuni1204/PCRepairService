using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using System.Numerics;

namespace AsyncStopwatchCA
{
    public class CAWorker
    {
        private readonly CAStopDBContext _context;
        private List<List<Timestamps>> _timestamps;

        public CAWorker(CAStopDBContext context)
        {
            _context = context;
            _timestamps = new List<List<Timestamps>>();
        }

        public void Work()
        {
            var entries = _context.Timestamps.ToList();
            if (entries.Count() > 0)
            {
                entries.OrderBy(o => o.Id);
                foreach (var entry in entries)
                {
                    
                }
            }
            _timestamps.Clear();
        }
    }
}
