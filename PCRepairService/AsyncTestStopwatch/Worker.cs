using AsyncTestStopwatch.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace AsyncTestStopwatch
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly StopwatchDBContext _context;

        public Worker(ILogger<Worker> logger, StopwatchDBContext context)
        {
            _logger = logger;
            _context = context;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                }
                await Task.Delay(1000, stoppingToken);
            }
        }

        private async Task Work()
        {
            
            var entries = await _context.Timestamps.ToListAsync();
            if (entries.Count() > 0)
            {
                foreach (var entry in entries)
                {
                    
                }
            }
            }
        }
    }
}
