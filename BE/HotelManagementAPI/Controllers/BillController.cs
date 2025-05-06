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
    public class BillController : ControllerBase
    {
        private readonly HotelDbContext _context;

        public BillController(HotelDbContext context)
        {
            _context = context;
        }

        // GET: api/Bill
        [HttpGet]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<ActionResult<IEnumerable<object>>> GetBills()
        {
            return await _context.Bills
                .Include(b => b.Booking)
                .ThenInclude(b => b.Customer)
                .Include(b => b.Discount)
                .Include(b => b.Staff)
                .Select(b => new
                {
                    b.BillsID,
                    b.BookingID,
                    CustomerName = b.Booking.Customer.CustomerName,
                    DiscountName = b.Discount.Description,
                    StaffName = b.Staff.StaffName,
                    b.Total,
                    b.CheckBill
                })
                .ToListAsync();
        }

        // GET: api/Bill/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<object>> GetBill(int id)
        {
            var bill = await _context.Bills
                .Include(b => b.Booking)
                .ThenInclude(b => b.Customer)
                .Include(b => b.Discount)
                .Include(b => b.Staff)
                .Include(b => b.Booking.DetailBookings)
                .ThenInclude(db => db.Room)
                .ThenInclude(r => r.TypeRoom)
                .Include(b => b.Booking.DetailBookings)
                .ThenInclude(db => db.DetailServices)
                .ThenInclude(ds => ds.Service)
                .Where(b => b.BillsID == id)
                .Select(b => new
                {
                    b.BillsID,
                    b.BookingID,
                    CustomerName = b.Booking.Customer.CustomerName,
                    DiscountName = b.Discount.Description,
                    DiscountRoomValue = b.Discount.DiscountRoomVlaue,
                    DiscountServiceValue = b.Discount.DiscountSerciveVlaue,
                    StaffName = b.Staff.StaffName,
                    b.Total,
                    b.CheckBill,
                    Rooms = b.Booking.DetailBookings.Select(db => new
                    {
                        db.RoomID,
                        RoomType = db.Room.TypeRoom.Description,
                        db.DateIn,
                        db.DateOut,
                        db.Price,
                        Services = db.DetailServices.Select(ds => new
                        {
                            ServiceName = ds.Service.Description,
                            ds.Price,
                            ds.DateUse
                        }).ToList()
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (bill == null)
            {
                return NotFound();
            }

            // If user is a customer, check if the bill belongs to them
            if (User.IsInRole("Customer"))
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserID == userId);
                
                if (customer == null)
                {
                    return Forbid();
                }

                var booking = await _context.Bookings.FindAsync(bill.BookingID);
                if (booking == null || booking.CustomerID != customer.CustomerID)
                {
                    return Forbid();
                }
            }

            return bill;
        }

        // GET: api/Bill/customer
        [HttpGet("customer")]
        [Authorize(Roles = "Customer")]
        public async Task<ActionResult<IEnumerable<object>>> GetCustomerBills()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserID == userId);
            
            if (customer == null)
            {
                return NotFound("Customer not found");
            }

            return await _context.Bills
                .Include(b => b.Booking)
                .Include(b => b.Discount)
                .Include(b => b.Staff)
                .Where(b => b.Booking.CustomerID == customer.CustomerID)
                .Select(b => new
                {
                    b.BillsID,
                    b.BookingID,
                    DiscountName = b.Discount.Description,
                    StaffName = b.Staff.StaffName,
                    b.Total,
                    b.CheckBill
                })
                .ToListAsync();
        }

        // POST: api/Bill
        [HttpPost]
        [Authorize(Roles = "Staff")]
        public async Task<ActionResult<Bill>> CreateBill(BillRequest request)
        {
            // Validate booking exists
            var booking = await _context.Bookings
                .Include(b => b.DetailBookings)
                .ThenInclude(db => db.DetailServices)
                .FirstOrDefaultAsync(b => b.BookingID == request.BookingID);

            if (booking == null)
            {
                return NotFound("Booking not found");
            }

            // Check if a bill already exists for this booking
            if (await _context.Bills.AnyAsync(b => b.BookingID == request.BookingID))
            {
                return BadRequest("A bill already exists for this booking");
            }

            // Validate discount exists
            var discount = await _context.Discounts.FindAsync(request.DiscountID);
            if (discount == null)
            {
                return NotFound("Discount not found");
            }

            // Get staff ID from authenticated user
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var staff = await _context.Staffs.FirstOrDefaultAsync(s => s.UserID == userId);
            
            if (staff == null)
            {
                return NotFound("Staff not found");
            }

            // Calculate total
            double total = 0;

            // Calculate room costs
            foreach (var detailBooking in booking.DetailBookings)
            {
                var days = (detailBooking.DateOut - detailBooking.DateIn).TotalDays;
                total += detailBooking.Price * days * discount.DiscountRoomVlaue;

                // Calculate service costs
                foreach (var service in detailBooking.DetailServices)
                {
                    total += service.Price * discount.DiscountSerciveVlaue;
                }
            }

            // Create bill
            var bill = new Bill
            {
                BookingID = request.BookingID,
                DiscountID = request.DiscountID,
                Total = total,
                CheckBill = request.CheckBill,
                StaffID = staff.StaffID
            };

            _context.Bills.Add(bill);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetBill), new { id = bill.BillsID }, bill);
        }

        // PUT: api/Bill/5/check
        [HttpPut("{id}/check")]
        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> CheckBill(int id)
        {
            var bill = await _context.Bills.FindAsync(id);
            if (bill == null)
            {
                return NotFound();
            }

            // Update bill status to checked
            bill.CheckBill = true;
            _context.Entry(bill).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BillExists(id))
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

        private bool BillExists(int id)
        {
            return _context.Bills.Any(e => e.BillsID == id);
        }
    }

    public class BillRequest
    {
        public int BookingID { get; set; }
        public int DiscountID { get; set; }
        public bool CheckBill { get; set; }
    }
}
