using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using PCRepairService.Interfaces;
using PCRepairService.Models;

namespace PCRepairService.DataAccess
{
    public class DA_AISO : IDA_AISO
    {
        private readonly ServiceDBContext _context;

        public DA_AISO(ServiceDBContext context)
        {
            _context = context;
        }

        public async Task AddAsync(AISO aiso)
        {
            await _context.AISO.AddAsync(aiso);
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
            var AISO = await _context.AISO.FindAsync(id);
            if (AISO != null)
            {
                _context.AISO.Remove(AISO);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<AISO>> GetAllAsync()
        {
            return await _context.AISO.ToListAsync();
        }

        public async Task<AISO> GetByIdAsync(long id)
        {
            return await _context.AISO.FindAsync(id);
        }
    }
}
