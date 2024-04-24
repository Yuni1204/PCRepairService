using Microsoft.EntityFrameworkCore;
using MessengerLibrary;
using OutboxWorker.DataAccess;

namespace OutboxWorker
{
    public class OutboxHandler : BackgroundService
    {
        private readonly ILogger<OutboxHandler> _logger;
        private readonly IDbContextFactory<OutboxDBContext> _dbContextFactory;
        private readonly IMessenger _messaging;

        public OutboxHandler(ILogger<OutboxHandler> logger, IDbContextFactory<OutboxDBContext> factory, IMessenger messaging)
        {
            _logger = logger;
            _dbContextFactory = factory;
            _messaging = messaging;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation($"Worker started at: {DateTimeOffset.Now.AddMilliseconds}");
            }
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation($"Worker handling at: {DateTimeOffset.Now.AddMilliseconds}");
                await HandleOutboxAsync();
                await Task.Delay(5000, stoppingToken);
            }
        }

        private async Task HandleOutboxAsync()
        {
            using (var dbContext = _dbContextFactory.CreateDbContext())
            {
                var messages = await dbContext.OutboxMessages.ToListAsync();
                if (messages.Count() > 0)
                {
                    foreach (var message in messages)
                    {   //send message
                        await _messaging.SendMessageAsync(message);
                        //delete from table
                        dbContext.OutboxMessages.Remove(message);
                        await dbContext.SaveChangesAsync();
                        _logger.LogInformation($"[SagaId {message.SagaId}] Sent and removed at {DateTime.UtcNow.ToLongTimeString()}");
                    }
                }
                else
                {
                    _logger.LogInformation($"No Messages in Outbox rn, sleeping...");
                }
                
            }
        }
    }
}
