using PCRepairService.Models;

namespace PCRepairService.Interfaces
{
    public interface ISagaHandler
    {
        Task StartServiceOrderSagaAsync(ServiceOrder serviceOrder);
        Task ConfirmAppointment(ServiceOrder serviceOrder, long sagaId);
        Task CompensateConfirmAppointmentFail(ServiceOrder serviceOrder, long sagaId);
        Task ReserveSpareCar(ServiceOrder serviceOrder, long sagaId);
        Task CompensateReserveSpareCarFail(ServiceOrder serviceOrder, long sagaId);
        Task EndServiceOrderSagaAsync(ServiceOrder serviceOrder, long id);
    }
}
