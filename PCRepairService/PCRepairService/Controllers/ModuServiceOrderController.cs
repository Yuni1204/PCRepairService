using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PCRepairService.DataAccess;
using PCRepairService.Interfaces;
using PCRepairService.Models;
using Swashbuckle.AspNetCore.Annotations;
using static System.Net.Mime.MediaTypeNames;

namespace PCRepairService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ModuServiceOrderController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IDA_ServiceOrder _DAServiceOrder;
        private readonly IDA_Timestamps _DATimestamps;
        private readonly ISagaHandler _SagaHandler;
        private readonly ServiceDBContext _context;

        public IRepairTimer _repairTimer;

        public ModuServiceOrderController(ServiceDBContext context, IDA_ServiceOrder so, ILogger<ModuServiceOrderController> logger, 
            ISagaHandler sagaHandler, IDA_Timestamps ts, IRepairTimer repairtimer)
        {
            _context = context;
            _DAServiceOrder = so;
            _logger = logger;
            _SagaHandler = sagaHandler;
            _DATimestamps = ts;
            _repairTimer = repairtimer;
        }

        // GET: api/ServiceOrder
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ServiceOrder>>> GetServiceOrder()
        {
            _logger.LogInformation("GetServiceOrder Requested");
            var result = await _DAServiceOrder.GetAllAsync();

            return Ok(result);
        }

        // GET: api/ServiceOrder/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ServiceOrder>> GetServiceOrder(long id)
        {
            _logger.LogInformation("GetServiceOrderById Requested");
            var ServiceOrder = await _DAServiceOrder.GetByIdAsync(id);

            if (ServiceOrder == null)
            {
                return NotFound();
            }

            return ServiceOrder;
        }

        // PUT: api/ServiceOrder/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutServiceOrder(long id, ServiceOrder ServiceOrder)
        {
            if (id != ServiceOrder.Id)
            {
                return BadRequest();
            }

            _context.Entry(ServiceOrder).State = EntityState.Modified;

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

        // POST: api/ServiceOrder
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<ServiceOrder>> PostServiceOrder(ServiceOrder ServiceOrder)
        {
            Thread.Sleep(10);
            _logger.LogInformation("PostServiceOrder Requested");
            //imagine validation

            await _DAServiceOrder.AddAsync(ServiceOrder);
            //, "ServiceOrders", "ServiceOrderCreated"
            //send message manually

            return CreatedAtAction("GetServiceOrder", new { id = ServiceOrder.Id }, ServiceOrder);
        }

        [HttpPost("outbox")]
        public async Task<ActionResult<ServiceOrder>> PostServiceOrderOutbox(ServiceOrder ServiceOrder)
        {
            Thread.Sleep(10);
            _logger.LogInformation("PostServiceOrderOutbox Requested");
            //imagine validation

            await _DAServiceOrder.AddWithMessageAsync(ServiceOrder, "ServiceOrders", "ServiceOrderCreated");

            return CreatedAtAction("GetServiceOrder", new { id = ServiceOrder.Id }, ServiceOrder);
        }

        [HttpPost("saga")]
        //[Route("saga")]
        [SwaggerOperation("CreateServiceOrder")]
        public async Task<ActionResult<ServiceOrder>> PostServiceOrderSaga(ServiceOrder ServiceOrder)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            Thread.Sleep(10);
            //_logger.LogInformation("PostServiceOrderSaga Requested");
            //imagine validation

            await _SagaHandler.StartServiceOrderSagaAsync(ServiceOrder);

            //await _DAServiceOrder.AddWithMessageAsync(ServiceOrder, "ServiceOrders", "ServiceOrderCreated");

            //await Task.WhenAll(task1);

            stopwatch.Stop();
            TimeSpan ts = stopwatch.Elapsed;
            var newstoptime = new RepairStopTime
            {
                ServiceOrderId = ServiceOrder.Id,
                StopTime = ts.Milliseconds
            };
            _repairTimer.AddStoppedTime(newstoptime);
            _logger.LogInformation($"[#SAGA] ID? {ServiceOrder.Id} POST-Request: {String.Format("{0:00000}", ts.Milliseconds)}");
            return CreatedAtAction("GetServiceOrder", new { id = ServiceOrder.Id }, ServiceOrder);
        }

        [HttpPost("sagaV2")]
        [SwaggerOperation("CreateServiceOrder")]
        public async Task<ActionResult<ServiceOrder>> TrySaga(ServiceOrder ServiceOrder)
        {
            _logger.LogInformation("TrySaga Requested");
            //imagine validation
            //REST calls to get information of appointments + car replacement availability
            //take first recommended appointment
            //another check if still available?
            //send messages to microservices for approval
            //what can go wrong? 
            //always in between REST and actually trying to book, might happen something
            //

            await _SagaHandler.StartServiceOrderSagaAsync(ServiceOrder);

            //await _DAServiceOrder.AddWithMessageAsync(ServiceOrder, "ServiceOrders", "ServiceOrderCreated");

            return CreatedAtAction("GetServiceOrder", new { id = ServiceOrder.Id }, ServiceOrder);
        }

        // DELETE: api/ServiceOrder/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteServiceOrder(long id)
        {
            _logger.LogInformation("DeleteServiceOrder Requested");
            var ServiceOrder = await _context.ServiceOrders.FindAsync(id);
            if (ServiceOrder == null)
            {
                return NotFound();
            }

            _context.ServiceOrders.Remove(ServiceOrder);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ServiceOrderExists(long id)
        {
            return _context.ServiceOrders.Any(e => e.Id == id);
        }
    }
}
