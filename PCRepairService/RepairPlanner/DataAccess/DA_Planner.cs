using Microsoft.EntityFrameworkCore;
using PCRepairService.Models;
using RepairPlanner.Models;

namespace RepairPlanner.DataAccess
{
    public class DA_Planner : IDA_Planner
    {
        private readonly PlannerDBContext _dbContext;
        private readonly ILogger _logger;

        public DA_Planner(PlannerDBContext context, ILogger<DA_Planner> logger)
        {
            _dbContext = context;
            _logger = logger;
        }

        public void AddServiceOrder(PServiceOrder serviceOrder)
        {
            _dbContext.ServiceOrders.Add(serviceOrder);
            _dbContext.SaveChanges();
        }

        public PServiceOrder? GetServiceOrder(long id)
        {
            return _dbContext.ServiceOrders.Find(id);
        }

        public void AddServiceOrderAppointments(PServiceOrder serviceOrder, PServiceOrder dbentry)
        {
            dbentry.HandoverAppointment = serviceOrder.HandoverAppointment;
            dbentry.ReturnDate = serviceOrder.ReturnDate;
            _dbContext.ServiceOrders.Entry(dbentry).State = EntityState.Modified;
            _dbContext.SaveChanges();
        }

        public void AddServiceOrderSpareCar(PServiceOrder dbentry)
        {
            dbentry.SpareCar = true;
            _dbContext.ServiceOrders.Entry(dbentry).State = EntityState.Modified;
            _dbContext.SaveChanges();
        }

        public void EditServiceOrder(PServiceOrder serviceOrder)
        {
            var dbEntry = _dbContext.ServiceOrders.Find(serviceOrder.Id);
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
                _dbContext.ServiceOrders.Entry(dbEntry).State = EntityState.Modified;
                _dbContext.SaveChanges();
            }
        }


    }
}
