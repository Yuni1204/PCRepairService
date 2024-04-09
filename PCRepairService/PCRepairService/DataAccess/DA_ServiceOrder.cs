using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using PCRepairService.Interfaces;
using PCRepairService.Models;
using Microsoft.CodeAnalysis.Text;
using System.Text.Json;
using MessengerLibrary;

namespace PCRepairService.DataAccess
{
    public class DA_ServiceOrder : IDA_ServiceOrder
    {
        private readonly ServiceDBContext _context;
        private readonly ILogger _logger;

        public DA_ServiceOrder(ServiceDBContext context, ILogger<DA_ServiceOrder> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task AddAsync(ServiceOrder ServiceOrder)
        {
            await _context.ServiceOrders.AddAsync(ServiceOrder);
            await _context.SaveChangesAsync();
            //_logger.LogInformation($"added ServiceOrder: {ServiceOrder} at {DateTimeOffset.Now}");
        }

        public async Task AddWithMessageAsync(ServiceOrder ServiceOrder, string exchange, string messageType)
        {
            var message = new Message
            {
                exchange = exchange,
                messageType = messageType,
                content = JsonSerializer.Serialize(ServiceOrder),
                Timestamp = DateTime.UtcNow
            };
            await _context.ServiceOrders.AddAsync(ServiceOrder);
            //_logger.LogInformation($"added ServiceOrder: {ServiceOrder} at {DateTimeOffset.Now}");
            await _context.OutboxMessages.AddAsync(message);
            //_logger.LogInformation($"added message: {message} at {DateTimeOffset.Now}");

            await _context.SaveChangesAsync();
        }

        //public async Task CreateServiceOrder(ServiceOrder serviceOrder, long kundeId)
        //{
        //    using (var context = new AppDBContext(_configuration))
        //    {
        //        serviceOrder.KundeId = kundeId;
        //        //context.Entry(serviceOrder).State = EntityState.Modified;

        //        int x = await (context.SaveChangesAsync());
        //    }
        //}

        public async void DeleteAsync(long id)
        {
            var ServiceOrder = await _context.ServiceOrders.FindAsync(id);
            if (ServiceOrder != null)
            {
                _context.ServiceOrders.Remove(ServiceOrder);
                await _context.SaveChangesAsync();
                //_logger.LogInformation($"deleted ServiceOrder: {ServiceOrder} at {DateTimeOffset.Now}");
            }
        }

        public async Task<IEnumerable<ServiceOrder>> GetAllAsync()
        {
            return await _context.ServiceOrders.ToListAsync();
        }

        public async Task<ServiceOrder?> GetByIdAsync(long id)
        {
            return await _context.ServiceOrders.FindAsync(id);
        }
    }
}
