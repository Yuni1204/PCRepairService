using MessengerLibrary;
using PCRepairService.DataAccess;
using PCRepairService.Interfaces;
using PCRepairService.Models;

namespace PCRepairService
{
    public class SagaHandler : ISagaHandler
    {
        private readonly ILogger _logger;
        private readonly IDA_ServiceOrder _daServiceOrder;
        //private readonly IMessenger _messenger;

        public SagaHandler(ILogger<SagaHandler> logger, IDA_ServiceOrder daServiceOrder/*, IMessenger messenger*/) 
        { 
            _logger = logger;
            _daServiceOrder = daServiceOrder;
            //_messenger = messenger;
        }

        public async Task StartServiceOrderSagaAsync(ServiceOrder serviceOrder)
        {

            await _daServiceOrder.AddWithMessageAsync(serviceOrder, "ServiceOrders", "ServiceOrderCreated", true);

            await Task.CompletedTask;
        }

        public async Task EndServiceOrderSagaAsync(long id)
        {
            await _daServiceOrder.EditSagaAsync(id); //bisher setzt er endserviceorder saga von alleine
        }
    }
}
