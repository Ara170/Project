using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using HotelManagementAPI.Data;
using HotelManagementAPI.Models;

namespace HotelManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TypeRoomController : ControllerBase
    {
        private readonly HotelDbContext _context;

        public TypeRoomController(HotelDbContext context)
        {
            _context = context;
        }

        // GET: api/TypeRoom
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TypeRoom>>> GetTypeRooms()
        {
            return await _context.TypeRooms.ToListAsync();
        }

        // GET: api/TypeRoom/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TypeRoom>> GetTypeRoom(int id)
        {
            var typeRoom = await _context.TypeRooms.FindAsync(id);

            if (typeRoom == null)
            {
                return NotFound();
            }

            return typeRoom;
        }

        // POST: api/TypeRoom
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<TypeRoom>> CreateTypeRoom(TypeRoom typeRoom)
        {
            // Check if type already exists
            if (await _context.TypeRooms.AnyAsync(t => t.Description == typeRoom.Description))
            {
                return BadRequest("This room type already exists");
            }

            // Check if price is valid
            if (typeRoom.Price <= 0)
            {
                return BadRequest("Price must be greater than 0");
            }

            _context.TypeRooms.Add(typeRoom);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTypeRoom), new { id = typeRoom.TypeID }, typeRoom);
        }

        // PUT: api/TypeRoom/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateTypeRoom(int id, TypeRoom typeRoom)
        {
            if (id != typeRoom.TypeID)
            {
                return BadRequest();
            }

            // Check if the new name already exists for a different type
            var existingType = await _context.TypeRooms
                .FirstOrDefaultAsync(t => t.Description == typeRoom.Description && t.TypeID != id);
                
            if (existingType != null)
            {
                return BadRequest("This room type name is already in use");
            }

            // Check if price is valid
            if (typeRoom.Price <= 0)
            {
                return BadRequest("Price must be greater than 0");
            }

            _context.Entry(typeRoom).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TypeRoomExists(id))
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

        // DELETE: api/TypeRoom/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteTypeRoom(int id)
        {
            var typeRoom = await _context.TypeRooms.FindAsync(id);
            if (typeRoom == null)
            {
                return NotFound();
            }

            // Check if there are rooms using this type
            var roomsUsingType = await _context.Rooms.AnyAsync(r => r.RoomType == id);
            if (roomsUsingType)
            {
                return BadRequest("Cannot delete this room type because it is being used by one or more rooms");
            }

            _context.TypeRooms.Remove(typeRoom);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TypeRoomExists(int id)
        {
            return _context.TypeRooms.Any(e => e.TypeID == id);
        }
    }
}
