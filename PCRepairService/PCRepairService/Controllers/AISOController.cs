using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PCRepairService.DataAccess;
using PCRepairService.Interfaces;
using PCRepairService.Models;

namespace PCRepairService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AISOController : ControllerBase
    {
        private readonly IDA_AISO _DAAiso;
        private readonly ServiceDBContext _context;

        public AISOController(ServiceDBContext context, IDA_AISO aiso)
        {
            _context = context;
            _DAAiso = aiso;
        }

        // GET: api/AISO
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AISO>>> GetAISO()
        {
            var result = await _DAAiso.GetAllAsync();

            var messenger = new Messaging();
            messenger.DoSomething(["hello"]);

            return Ok(result);
        }

        // GET: api/AISO/5
        [HttpGet("{id}")]
        public async Task<ActionResult<AISO>> GetAISO(long id)
        {
            var aISO = await _DAAiso.GetByIdAsync(id);

            if (aISO == null)
            {
                return NotFound();
            }

            return aISO;
        }

        // PUT: api/AISO/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        //[HttpPut("{id}")]
        //public async Task<IActionResult> PutAISO(long id, AISO aISO)
        //{
        //    if (id != aISO.Id)
        //    {
        //        return BadRequest();
        //    }

        //    _context.Entry(aISO).State = EntityState.Modified;

        //    try
        //    {
        //        await _context.SaveChangesAsync();
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        if (!AISOExists(id))
        //        {
        //            return NotFound();
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }

        //    return NoContent();
        //}

        // POST: api/AISO
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<AISO>> PostAISO(AISO aISO)
        {
            await _DAAiso.AddAsync(aISO);

            return CreatedAtAction("GetAISO", new { id = aISO.Id }, aISO);
        }

        // DELETE: api/AISO/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAISO(long id)
        {
            var aISO = await _context.AISO.FindAsync(id);
            if (aISO == null)
            {
                return NotFound();
            }

            _context.AISO.Remove(aISO);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool AISOExists(long id)
        {
            return _context.AISO.Any(e => e.Id == id);
        }
    }
}
