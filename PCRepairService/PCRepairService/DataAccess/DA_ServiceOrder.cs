using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using PCRepairService.Interfaces;
using PCRepairService.Models;
using Microsoft.CodeAnalysis.Text;
using System.Text.Json;
using MessengerLibrary;
using static System.Net.Mime.MediaTypeNames;

namespace PCRepairService.DataAccess
{
    public class DA_ServiceOrder : IDA_ServiceOrder
    {
        private readonly ServiceDBContext _context;
        private readonly ILogger _logger;
        private readonly IDA_Timestamps _timestamps;

        public DA_ServiceOrder(ServiceDBContext context, ILogger<DA_ServiceOrder> logger, IDA_Timestamps ts)
        {
            _context = context;
            _logger = logger;
            _timestamps = ts;
        }

        public async Task AddAsync(ServiceOrder serviceOrder)
        {
            serviceOrder.Description = "noPatterns " + serviceOrder.Description;
            await _context.ServiceOrders.AddAsync(serviceOrder);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"added ServiceOrder: {serviceOrder} at {DateTimeOffset.Now.ToString("hh.mm.ss.ffffff")}");
        }

        public async Task AddWithMessageAsync(ServiceOrder serviceOrder, string exchange, string messageType)
        {
            serviceOrder.Description = "OnlyOutboxMessaging " + serviceOrder.Description;
            await _context.ServiceOrders.AddAsync(serviceOrder);
            _logger.LogInformation($"added ServiceOrder: {serviceOrder} at {DateTimeOffset.Now.ToString("hh.mm.ss.ffffff")}");
            await _context.SaveChangesAsync();
            var message = new Message
            {
                Id = serviceOrder.Id,
                exchange = exchange,
                messageType = messageType,
                content = JsonSerializer.Serialize(serviceOrder),
                Timestamp = DateTime.UtcNow
            };
            await _context.OutboxMessages.AddAsync(message);
            _logger.LogInformation($"added message: {message} at {DateTimeOffset.Now.ToString("hh.mm.ss.ffffff")}");
            await _context.SaveChangesAsync();
            _logger.LogInformation($"SaveChangesAsync at {DateTimeOffset.Now.ToString("hh.mm.ss.ffffff")}");
        }

        public async Task<long> CreateSagaAsync(String nextSaga)
        {
            var sagalog = new SagaServiceOrder
            {
                NextStep = nextSaga,
                ServiceOrderCreated = true
            };
            await _context.ServiceOrderSagaLog.AddAsync(sagalog);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"[#SAGA {sagalog.Id}] Started Saga at {DateTimeOffset.Now.ToString("hh.mm.ss.ffffff")}");
            await _timestamps.AddAsync(DateTimeOffset.Now.ToString("hh.mm.ss.ffffff"), sagalog.Id);
            return sagalog.Id;
        }

        public async Task SagaAddWithMessageAsync(ServiceOrder serviceOrder, string exchange, string messageType, 
                                                    string nextSaga, long sagaId)
        {
            serviceOrder.Description = "SagaPatternOutbox " + serviceOrder.Description;
            await _context.ServiceOrders.AddAsync(serviceOrder);
            _logger.LogInformation($"[#SAGA {sagaId}] added ServiceOrder at {DateTimeOffset.Now.ToString("hh.mm.ss.ffffff")}");
            await _context.SaveChangesAsync();
            var message = new Message
            {
                Id = serviceOrder.Id,
                exchange = exchange,
                messageType = messageType,
                content = JsonSerializer.Serialize(serviceOrder),
                Timestamp = DateTime.UtcNow,
                SagaId = sagaId
            };            
            await _context.OutboxMessages.AddAsync(message);
            //_logger.LogInformation($"added message at {DateTimeOffset.Now.ToString("hh.mm.ss.ffffff")}");
            await _context.SaveChangesAsync();
            _logger.LogInformation($"[#SAGA {sagaId}] SagaAddWithMessageAsync SaveChangesAsync at {DateTimeOffset.Now.ToString("hh.mm.ss.ffffff")}");
            await EditSagaAsync(sagaId, nextSaga, false);
        }

        public async Task SagaMessageAsync(ServiceOrder serviceOrder, string exchange, string messageType,
                                                    string nextSaga, long sagaId, bool compensate)
        {
            var message = new Message
            {
                Id = serviceOrder.Id,
                exchange = exchange,
                messageType = messageType,
                content = JsonSerializer.Serialize(serviceOrder),
                Timestamp = DateTime.UtcNow,
                SagaId = sagaId
            };
            if(messageType != "_")
            {
                await _context.OutboxMessages.AddAsync(message);
                //_logger.LogInformation($"added message at {DateTimeOffset.Now.ToString("hh.mm.ss.ffffff")}");
                await _context.SaveChangesAsync();
            }
            _logger.LogInformation($"[#SAGA {sagaId}] SagaMessageAsync SaveChangesAsync at {DateTimeOffset.Now.ToString("hh.mm.ss.ffffff")}");
            await EditSagaAsync(sagaId, nextSaga, compensate);
        }

        public async Task EditSagaAsync(long id, string nextstep, bool compensate)
        {
            var dbEntry = await _context.ServiceOrderSagaLog.FindAsync(id);
            if (dbEntry != null)
            {
                //dbEntry.NextStep = null;
                //dbEntry.AppointmentDatesConfirmed = true;

                switch(dbEntry.NextStep)
                {
                    case "ConfirmAppointment":
                        // created, appointment sent
                        dbEntry.AppointmentDatesConfirmed = true;
                        dbEntry.NextStep = nextstep;
                        break;
                    case "SpareCar":
                        //created, appointment confirmed, spareCar reserving
                        if (compensate)
                        {
                            dbEntry.AppointmentDatesConfirmed = false;
                            dbEntry.NextStep = (nextstep == "null") ? null : nextstep;
                            break;
                        }
                        dbEntry.AppointmentDatesConfirmed = true;
                        dbEntry.NextStep = nextstep;
                        break;
                    case "AppointmentReset":
                        dbEntry.AppointmentDatesConfirmed = false;
                        dbEntry.NextStep = (nextstep == "null") ? null : nextstep;
                        break;
                    case "finish":
                        if(compensate)
                        {
                            dbEntry.SpareCarReserved = false;
                            dbEntry.NextStep = nextstep;
                            break;
                        }
                        dbEntry.SpareCarReserved = true;
                        dbEntry.NextStep = null;
                        break;
                    //case ""
                    default:
                        throw new Exception();
                        //break;
                }

                _context.ServiceOrderSagaLog.Entry(dbEntry).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                _logger.LogInformation($"[#SAGA {id}] EditSagaAsync SaveChangesAsync at {DateTimeOffset.Now.ToString("hh.mm.ss.ffffff")}");
            }
        }



        public async void DeleteAsync(long id)
        {
            var ServiceOrder = await _context.ServiceOrders.FindAsync(id);
            if (ServiceOrder != null)
            {
                _context.ServiceOrders.Remove(ServiceOrder);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"deleted ServiceOrder: {ServiceOrder} at {DateTimeOffset.Now.ToString("hh.mm.ss.ffffff")}");
            }
        }

        public async Task EditAsync(ServiceOrder serviceOrder)
        {
            var dbEntry = await _context.ServiceOrders.FindAsync(serviceOrder.Id);
            if (dbEntry != null)
            {
                dbEntry.ServiceOrderType = serviceOrder.ServiceOrderType;
                dbEntry.Description = serviceOrder.Description;
                dbEntry.Name = serviceOrder.Name;
                dbEntry.Cost = serviceOrder.Cost;
                dbEntry.IsCompleted = serviceOrder.IsCompleted;
                dbEntry.HandoverAppointment = serviceOrder.HandoverAppointment;
                dbEntry.ReturnDate = serviceOrder.ReturnDate;
                dbEntry.SpareCar = serviceOrder.SpareCar;
                _context.ServiceOrders.Entry(dbEntry).State = EntityState.Modified;
                await _context.SaveChangesAsync();
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
