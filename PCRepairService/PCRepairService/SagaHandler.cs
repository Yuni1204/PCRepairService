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
            var sagaId = await _daServiceOrder.CreateSagaAsync("ConfirmAppointment");
            await ConfirmAppointment(serviceOrder, sagaId);
            //await _daServiceOrder.AddWithMessageAsync(serviceOrder, "ServiceOrders", "ServiceOrderCreated", true);
            
        }

        public async Task ConfirmAppointment(ServiceOrder serviceOrder, long sagaId)
        {
            await _daServiceOrder.SagaAddWithMessageAsync(serviceOrder, "ServiceOrders", "AppointmentSelected", "SpareCar", sagaId);
        }

        public async Task CompensateConfirmAppointmentFail(ServiceOrder serviceOrder, long sagaId)
        {
            serviceOrder.ReturnDate = null;
            serviceOrder.HandoverAppointment = null;
            await _daServiceOrder.EditAsync(sagaId, "null", serviceOrder);
            await _daServiceOrder.SagaMessageAsync(serviceOrder, "ServiceOrders", "_", "null", sagaId, true);
        }

        public async Task ReserveSpareCar(ServiceOrder serviceOrder, long sagaId)
        {
            await _daServiceOrder.SagaMessageAsync(serviceOrder, "ServiceOrders", "ReserveSpareCar", "finish", sagaId, false);
        }

        public async Task CompensateReserveSpareCarFail(ServiceOrder serviceOrder, long sagaId)
        {
            await _daServiceOrder.SagaMessageAsync(serviceOrder, "ServiceOrders", "CancelAppointment", "AppointmentReset", sagaId, true);
        }

        public async Task EndServiceOrderSagaAsync(ServiceOrder serviceOrder, long id)
        {
            await _daServiceOrder.EditSagaAsync(id, "null", false); //bisher setzt er endserviceorder saga von alleine
        }
    }
}
