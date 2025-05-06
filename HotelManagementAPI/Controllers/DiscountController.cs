using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using HotelManagementAPI.Data;
using HotelManagementAPI.Models;

namespace HotelManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DiscountController : ControllerBase
    {
        private readonly HotelDbContext _context;

        public DiscountController(HotelDbContext context)
        {
            _context = context;
        }

        // GET: api/Discount
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Discount>>> GetDiscounts()
        {
            return await _context.Discounts.ToListAsync();
        }

        // GET: api/Discount/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Discount>> GetDiscount(int id)
        {
            var discount = await _context.Discounts.FindAsync(id);

            if (discount == null)
            {
                return NotFound();
            }

            return discount;
        }

        // POST: api/Discount
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Discount>> CreateDiscount(Discount discount)
        {
            // Check if discount already exists
            if (await _context.Discounts.AnyAsync(d => d.Description == discount.Description))
            {
                return BadRequest("This discount already exists");
            }

            // Check if discount values are valid
            if (discount.DiscountRoomVlaue <= 0 || discount.DiscountSerciveVlaue <= 0)
            {
                return BadRequest("Discount values must be greater than 0");
            }

            _context.Discounts.Add(discount);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetDiscount), new { id = discount.DiscountID }, discount);
        }

        // PUT: api/Discount/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateDiscount(int id, Discount discount)
        {
            if (id != discount.DiscountID)
            {
                return BadRequest();
            }

            // Check if the new name already exists for a different discount
            var existingDiscount = await _context.Discounts
                .FirstOrDefaultAsync(d => d.Description == discount.Description && d.DiscountID != id);
                
            if (existingDiscount != null)
            {
                return BadRequest("This discount name is already in use");
            }

            // Check if discount values are valid
            if (discount.DiscountRoomVlaue <= 0 || discount.DiscountSerciveVlaue <= 0)
            {
                return BadRequest("Discount values must be greater than 0");
            }

            _context.Entry(discount).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DiscountExists(id))
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

        // DELETE: api/Discount/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteDiscount(int id)
        {
            var discount = await _context.Discounts.FindAsync(id);
            if (discount == null)
            {
                return NotFound();
            }

            // Check if discount is in use (has bills)
            var discountInUse = await _context.Bills.AnyAsync(b => b.DiscountID == id);
            if (discountInUse)
            {
                return BadRequest("Cannot delete this discount because it is being used by one or more bills");
            }

            _context.Discounts.Remove(discount);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool DiscountExists(int id)
        {
            return _context.Discounts.Any(e => e.DiscountID == id);
        }
    }
}
