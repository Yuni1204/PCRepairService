using RepairPlanner.Models;

namespace RepairPlanner.DataAccess
{
    public interface IDA_Planner
    {
        void AddServiceOrder(PServiceOrder serviceOrder);
        PServiceOrder? GetServiceOrder(long id);
        void AddServiceOrderAppointments(PServiceOrder serviceOrder, PServiceOrder dbentry);
        void AddServiceOrderSpareCar(PServiceOrder dbentry);
        void EditServiceOrder(PServiceOrder serviceOrder);

    }
}
