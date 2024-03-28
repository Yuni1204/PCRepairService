using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PCRepairService.Models;

namespace PCRepairService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceOrdersController : ControllerBase
    {
        private readonly ServiceDBContext _context;

        public ServiceOrdersController(ServiceDBContext context)
        {
            _context = context;
        }

        // GET: api/ServiceOrders
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ServiceOrder>>> GetServiceOrder()
        {
            return await _context.ServiceOrder.ToListAsync();
        }

        // GET: api/ServiceOrders/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ServiceOrder>> GetServiceOrder(long id)
        {
            var serviceOrder = await _context.ServiceOrder.FindAsync(id);

            if (serviceOrder == null)
            {
                return NotFound();
            }

            return serviceOrder;
        }

        // PUT: api/ServiceOrders/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutServiceOrder(long id, ServiceOrder serviceOrder)
        {
            if (id != serviceOrder.Id)
            {
                return BadRequest();
            }

            _context.Entry(serviceOrder).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ServiceOrderExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/ServiceOrders
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost("{kundeId}")]
        public async Task<ActionResult<ServiceOrder>> PostServiceOrder(ServiceOrder serviceOrder, long kundeId)
        {
            serviceOrder.KundeId = kundeId;

            _context.ServiceOrder.Add(serviceOrder);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetServiceOrder", new { id = serviceOrder.Id }, serviceOrder);
        }

        // DELETE: api/ServiceOrders/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteServiceOrder(long id)
        {
            var serviceOrder = await _context.ServiceOrder.FindAsync(id);
            if (serviceOrder == null)
            {
                return NotFound();
            }

            _context.ServiceOrder.Remove(serviceOrder);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ServiceOrderExists(long id)
        {
            return _context.ServiceOrder.Any(e => e.Id == id);
        }
    }
}
