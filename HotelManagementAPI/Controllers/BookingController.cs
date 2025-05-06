using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using HotelManagementAPI.Data;
using HotelManagementAPI.Models;
using System.Security.Claims;

namespace HotelManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly HotelDbContext _context;

        public BookingController(HotelDbContext context)
        {
            _context = context;
        }

        // GET: api/Booking
        [HttpGet]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<ActionResult<IEnumerable<object>>> GetAllBookings()
        {
            return await _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.DetailBookings)
                .ThenInclude(db => db.Room)
                .ThenInclude(r => r.TypeRoom)
                .Select(b => new
                {
                    b.BookingID,
                    CustomerName = b.Customer.CustomerName,
                    b.BookingDate,
                    RoomCount = b.RoomBookingCount,
                    Rooms = b.DetailBookings.Select(db => new
                    {
                        db.RoomID,
                        RoomType = db.Room.TypeRoom.Description,
                        db.DateIn,
                        db.DateOut,
                        db.Price
                    }).ToList()
                })
                .ToListAsync();
        }

        // GET: api/Booking/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<object>> GetBooking(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.DetailBookings)
                .ThenInclude(db => db.Room)
                .ThenInclude(r => r.TypeRoom)
                .Where(b => b.BookingID == id)
                .Select(b => new
                {
                    b.BookingID,
                    CustomerName = b.Customer.CustomerName,
                    b.BookingDate,
                    RoomCount = b.RoomBookingCount,
                    Rooms = b.DetailBookings.Select(db => new
                    {
                        db.RoomID,
                        RoomType = db.Room.TypeRoom.Description,
                        db.DateIn,
                        db.DateOut,
                        db.Price
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (booking == null)
            {
                return NotFound();
            }

            // If user is a customer, check if the booking belongs to them
            if (User.IsInRole("Customer"))
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserID == userId);
                
                if (customer == null)
                {
                    return Forbid();
                }

                var customerBooking = await _context.Bookings
                    .AnyAsync(b => b.BookingID == id && b.CustomerID == customer.CustomerID);
                
                if (!customerBooking)
                {
                    return Forbid();
                }
            }

            return booking;
        }

        // GET: api/Booking/customer
        [HttpGet("customer")]
        [Authorize(Roles = "Customer")]
        public async Task<ActionResult<IEnumerable<object>>> GetCustomerBookings()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserID == userId);
            
            if (customer == null)
            {
                return NotFound("Customer not found");
            }

            return await _context.Bookings
                .Include(b => b.DetailBookings)
                .ThenInclude(db => db.Room)
                .ThenInclude(r => r.TypeRoom)
                .Where(b => b.CustomerID == customer.CustomerID)
                .Select(b => new
                {
                    b.BookingID,
                    b.BookingDate,
                    RoomCount = b.RoomBookingCount,
                    Rooms = b.DetailBookings.Select(db => new
                    {
                        db.RoomID,
                        RoomType = db.Room.TypeRoom.Description,
                        db.DateIn,
                        db.DateOut,
                        db.Price
                    }).ToList()
                })
                .ToListAsync();
        }

        // POST: api/Booking
        [HttpPost]
        [Authorize(Roles = "Customer")]
        public async Task<ActionResult<Booking>> CreateBooking([FromBody] BookingRequest request)
        {
            if (request.Rooms == null || !request.Rooms.Any())
            {
                return BadRequest("No rooms specified for booking");
            }

            // Get customer ID from authenticated user
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserID == userId);
            
            if (customer == null)
            {
                return NotFound("Customer not found");
            }

            // Check if rooms are available for the requested dates
            foreach (var roomRequest in request.Rooms)
            {
                // Validate room exists
                var room = await _context.Rooms.FindAsync(roomRequest.RoomID);
                if (room == null)
                {
                    return BadRequest($"Room {roomRequest.RoomID} does not exist");
                }

                // Validate dates
                if (roomRequest.DateIn >= roomRequest.DateOut)
                {
                    return BadRequest("Check-in date must be before check-out date");
                }

                if (roomRequest.DateIn.Date < DateTime.Now.Date)
                {
                    return BadRequest("Check-in date cannot be in the past");
                }

                // Check if room is available for the requested dates
                var isRoomBooked = await _context.DetailBookings
                    .AnyAsync(db => 
                        db.RoomID == roomRequest.RoomID && 
                        ((roomRequest.DateIn >= db.DateIn && roomRequest.DateIn < db.DateOut) ||
                         (roomRequest.DateOut > db.DateIn && roomRequest.DateOut <= db.DateOut) ||
                         (roomRequest.DateIn <= db.DateIn && roomRequest.DateOut >= db.DateOut)));

                if (isRoomBooked)
                {
                    return BadRequest($"Room {roomRequest.RoomID} is not available for the requested dates");
                }
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Create booking
                var booking = new Booking
                {
                    CustomerID = customer.CustomerID,
                    RoomBookingCount = request.Rooms.Count,
                    BookingDate = DateTime.Now
                };

                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();

                // Create detail bookings for each room
                foreach (var roomRequest in request.Rooms)
                {
                    var room = await _context.Rooms
                        .Include(r => r.TypeRoom)
                        .FirstOrDefaultAsync(r => r.RoomID == roomRequest.RoomID);

                    var detailBooking = new DetailBooking
                    {
                        BookingID = booking.BookingID,
                        RoomID = roomRequest.RoomID,
                        Price = room.TypeRoom.Price,
                        DateIn = roomRequest.DateIn,
                        DateOut = roomRequest.DateOut
                    };

                    _context.DetailBookings.Add(detailBooking);

                    // Update room state to Booked
                    room.State = "Booked";
                    _context.Entry(room).State = EntityState.Modified;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return CreatedAtAction(nameof(GetBooking), new { id = booking.BookingID }, booking);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // DELETE: api/Booking/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> CancelBooking(int id)
        {
            // Get customer ID from authenticated user
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserID == userId);
            
            if (customer == null)
            {
                return NotFound("Customer not found");
            }

            // Check if booking exists and belongs to the customer
            var booking = await _context.Bookings
                .Include(b => b.DetailBookings)
                .FirstOrDefaultAsync(b => b.BookingID == id && b.CustomerID == customer.CustomerID);

            if (booking == null)
            {
                return NotFound("Booking not found or does not belong to you");
            }

            // Check if there's already a bill for this booking
            var hasBill = await _context.Bills.AnyAsync(b => b.BookingID == id);
            if (hasBill)
            {
                return BadRequest("Cannot cancel a booking that has already been billed");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Update room states back to Available
                foreach (var detailBooking in booking.DetailBookings)
                {
                    var room = await _context.Rooms.FindAsync(detailBooking.RoomID);
                    if (room != null)
                    {
                        room.State = "Available";
                        _context.Entry(room).State = EntityState.Modified;
                    }

                    // Remove detail services if any
                    var detailServices = await _context.DetailServices
                        .Where(ds => ds.Booking_Service_ID == detailBooking.Booking_Service_ID)
                        .ToListAsync();

                    if (detailServices.Any())
                    {
                        _context.DetailServices.RemoveRange(detailServices);
                    }
                }

                // Remove detail bookings
                _context.DetailBookings.RemoveRange(booking.DetailBookings);

                // Remove booking
                _context.Bookings.Remove(booking);

                // Increment cancellation count for user
                var user = await _context.Users.FindAsync(customer.UserID);
                if (user != null)
                {
                    user.Cancellations += 1;
                    _context.Entry(user).State = EntityState.Modified;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return NoContent();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }

    public class BookingRequest
    {
        public List<RoomBookingRequest> Rooms { get; set; } = new List<RoomBookingRequest>();
    }

    public class RoomBookingRequest
    {
        public required string RoomID { get; set; }
        public DateTime DateIn { get; set; }
        public DateTime DateOut { get; set; }
    }
}
