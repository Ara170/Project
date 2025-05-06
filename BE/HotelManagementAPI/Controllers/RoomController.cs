using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using HotelManagementAPI.Data;
using HotelManagementAPI.Models;

namespace HotelManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoomController : ControllerBase
    {
        private readonly HotelDbContext _context;

        public RoomController(HotelDbContext context)
        {
            _context = context;
        }

        // GET: api/Room
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetRooms()
        {
            return await _context.Rooms
                .Include(r => r.TypeRoom)
                .Select(r => new
                {
                    r.RoomID,
                    TypeOfRoom = r.TypeRoom.Description,
                    r.State,
                    Price = r.TypeRoom.Price
                })
                .ToListAsync();
        }

        // GET: api/Room/5
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetRoom(string id)
        {
            var room = await _context.Rooms
                .Include(r => r.TypeRoom)
                .Where(r => r.RoomID == id)
                .Select(r => new
                {
                    r.RoomID,
                    TypeOfRoom = r.TypeRoom.Description,
                    r.State,
                    Price = r.TypeRoom.Price
                })
                .FirstOrDefaultAsync();

            if (room == null)
            {
                return NotFound();
            }

            return room;
        }

        // GET: api/Room/available
        [HttpGet("available")]
        public async Task<ActionResult<IEnumerable<object>>> GetAvailableRooms()
        {
            return await _context.Rooms
                .Include(r => r.TypeRoom)
                .Where(r => r.State == "Available")
                .Select(r => new
                {
                    r.RoomID,
                    TypeOfRoom = r.TypeRoom.Description,
                    r.State,
                    Price = r.TypeRoom.Price
                })
                .ToListAsync();
        }

        // POST: api/Room
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Room>> CreateRoom(Room room)
        {
            // Check if room ID is valid (3 characters)
            if (room.RoomID.Length != 3)
            {
                return BadRequest("Room ID must be exactly 3 characters");
            }

            // Check if room already exists
            if (await _context.Rooms.AnyAsync(r => r.RoomID == room.RoomID))
            {
                return BadRequest("This room already exists");
            }

            // Check if room type exists
            var typeRoom = await _context.TypeRooms.FindAsync(room.RoomType);
            if (typeRoom == null)
            {
                return BadRequest("Invalid room type");
            }

            // Set initial state to Available
            room.State = "Available";

            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetRoom), new { id = room.RoomID }, room);
        }

        // PUT: api/Room/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> UpdateRoom(string id, Room room)
        {
            if (id != room.RoomID)
            {
                return BadRequest();
            }

            // Check if room type exists
            var typeRoom = await _context.TypeRooms.FindAsync(room.RoomType);
            if (typeRoom == null)
            {
                return BadRequest("Invalid room type");
            }

            // Validate state
            if (room.State != "Available" && room.State != "Booked")
            {
                return BadRequest("Invalid room state. Must be 'Available' or 'Booked'");
            }

            _context.Entry(room).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RoomExists(id))
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

        // DELETE: api/Room/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteRoom(string id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null)
            {
                return NotFound();
            }

            // Check if room is in use (has bookings)
            var roomInUse = await _context.DetailBookings.AnyAsync(db => db.RoomID == id);
            if (roomInUse)
            {
                return BadRequest("Cannot delete this room because it has bookings");
            }

            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool RoomExists(string id)
        {
            return _context.Rooms.Any(e => e.RoomID == id);
        }
    }
}
