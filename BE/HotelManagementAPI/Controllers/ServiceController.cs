using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using HotelManagementAPI.Data;
using HotelManagementAPI.Models;

namespace HotelManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceController : ControllerBase
    {
        private readonly HotelDbContext _context;

        public ServiceController(HotelDbContext context)
        {
            _context = context;
        }

        // GET: api/Service
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Service>>> GetServices()
        {
            return await _context.Services.ToListAsync();
        }

        // GET: api/Service/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Service>> GetService(int id)
        {
            var service = await _context.Services.FindAsync(id);

            if (service == null)
            {
                return NotFound();
            }

            return service;
        }

        // POST: api/Service
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Service>> CreateService(Service service)
        {
            // Check if service already exists
            if (await _context.Services.AnyAsync(s => s.Description == service.Description))
            {
                return BadRequest("This service already exists");
            }

            // Check if price is valid
            if (service.Price <= 0)
            {
                return BadRequest("Price must be greater than 0");
            }

            _context.Services.Add(service);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetService), new { id = service.ServiceID }, service);
        }

        // PUT: api/Service/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateService(int id, Service service)
        {
            if (id != service.ServiceID)
            {
                return BadRequest();
            }

            // Check if the new name already exists for a different service
            var existingService = await _context.Services
                .FirstOrDefaultAsync(s => s.Description == service.Description && s.ServiceID != id);
                
            if (existingService != null)
            {
                return BadRequest("This service name is already in use");
            }

            // Check if price is valid
            if (service.Price <= 0)
            {
                return BadRequest("Price must be greater than 0");
            }

            _context.Entry(service).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ServiceExists(id))
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

        // DELETE: api/Service/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteService(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service == null)
            {
                return NotFound();
            }

            // Check if there are bookings using this service
            var serviceInUse = await _context.DetailServices.AnyAsync(ds => ds.ServiceID == id);
            if (serviceInUse)
            {
                return BadRequest("Cannot delete this service because it is being used by one or more bookings");
            }

            _context.Services.Remove(service);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ServiceExists(int id)
        {
            return _context.Services.Any(e => e.ServiceID == id);
        }
    }
}
