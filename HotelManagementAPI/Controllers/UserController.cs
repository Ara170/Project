using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using HotelManagementAPI.Data;
using HotelManagementAPI.Models;
using HotelManagementAPI.Services;
using HotelManagementAPI.DTOs;
using System.Security.Claims;

namespace HotelManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly HotelDbContext _context;
        private readonly IPasswordService _passwordService;

        public UserController(
            HotelDbContext context,
            IPasswordService passwordService)
        {
            _context = context;
            _passwordService = passwordService;
        }

        // GET: api/User/profile
        [HttpGet("profile")]
        [Authorize]
        public async Task<ActionResult<object>> GetUserProfile()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Forbid();
            }
            var userId = int.Parse(userIdClaim.Value);
            
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserID == userId);

            if (user == null)
            {
                return NotFound("User not found");
            }

            if (user.Role.Description == "Customer")
            {
                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.UserID == userId);

                if (customer == null)
                {
                    return NotFound("Customer profile not found");
                }

                return new
                {
                    user.Username,
                    Role = user.Role.Description,
                    customer.CustomerName,
                    customer.BirthDate,
                    customer.IDCard,
                    customer.Email,
                    customer.PhoneNumber,
                    customer.Address,
                    user.CreateAt,
                    user.State,
                    user.Cancellations
                };
            }
            else if (user.Role.Description == "Staff" || user.Role.Description == "Admin")
            {
                var staff = await _context.Staffs
                    .FirstOrDefaultAsync(s => s.UserID == userId);

                if (staff == null)
                {
                    return NotFound("Staff profile not found");
                }

                return new
                {
                    user.Username,
                    Role = user.Role.Description,
                    StaffName = staff.StaffName,
                    staff.BirthDate,
                    staff.IDCard,
                    staff.Email,
                    staff.PhoneNumber,
                    staff.Address,
                    user.CreateAt,
                    user.State,
                    user.Cancellations
                };
            }

            return BadRequest("Invalid user role");
        }

        // PUT: api/User/profile
        [HttpPut("profile")]
        [Authorize]
        public async Task<IActionResult> UpdateUserProfile(ProfileUpdateRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Forbid();
            }
            var userId = int.Parse(userIdClaim.Value);
            
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserID == userId);

            if (user == null)
            {
                return NotFound("User not found");
            }

            if (user.Role.Description == "Customer")
            {
                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.UserID == userId);

                if (customer == null)
                {
                    return NotFound("Customer profile not found");
                }

                // Check if email is already in use by another customer
                if (await _context.Customers.AnyAsync(c => c.Email == request.Email && c.CustomerID != customer.CustomerID))
                {
                    return BadRequest("Email is already in use");
                }

                // Check if phone number is already in use by another customer
                if (await _context.Customers.AnyAsync(c => c.PhoneNumber == request.PhoneNumber && c.CustomerID != customer.CustomerID))
                {
                    return BadRequest("Phone number is already in use");
                }

                // Update customer profile
                customer.CustomerName = request.Name;
                customer.BirthDate = request.BirthDate;
                customer.Email = request.Email;
                customer.PhoneNumber = request.PhoneNumber;
                customer.Address = request.Address;

                _context.Entry(customer).State = EntityState.Modified;
            }
            else if (user.Role.Description == "Staff" || user.Role.Description == "Admin")
            {
                var staff = await _context.Staffs
                    .FirstOrDefaultAsync(s => s.UserID == userId);

                if (staff == null)
                {
                    return NotFound("Staff profile not found");
                }

                // Check if email is already in use by another staff
                if (await _context.Staffs.AnyAsync(s => s.Email == request.Email && s.StaffID != staff.StaffID))
                {
                    return BadRequest("Email is already in use");
                }

                // Check if phone number is already in use by another staff
                if (await _context.Staffs.AnyAsync(s => s.PhoneNumber == request.PhoneNumber && s.StaffID != staff.StaffID))
                {
                    return BadRequest("Phone number is already in use");
                }

                // Update staff profile
                staff.StaffName = request.Name;
                staff.BirthDate = request.BirthDate;
                staff.Email = request.Email;
                staff.PhoneNumber = request.PhoneNumber;
                staff.Address = request.Address;

                _context.Entry(staff).State = EntityState.Modified;
            }
            else
            {
                return BadRequest("Invalid user role");
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return StatusCode(500, "An error occurred while updating the profile");
            }

            return NoContent();
        }

        // PUT: api/User/password
        [HttpPut("password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword(PasswordChangeRequest request)
        {
            if (request.NewPassword.Length < 10)
            {
                return BadRequest("New password must be at least 10 characters long");
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Forbid();
            }
            var userId = int.Parse(userIdClaim.Value);
            
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return NotFound("User not found");
            }

            // Verify current password
            bool isPasswordValid = _passwordService.VerifyPassword(
                request.CurrentPassword, 
                user.Password, 
                user.Salt);

            if (!isPasswordValid)
            {
                return BadRequest("Current password is incorrect");
            }

            // Hash new password
            var (passwordHash, salt) = _passwordService.HashPassword(request.NewPassword);

            // Update password
            user.Password = passwordHash;
            user.Salt = salt;
            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return StatusCode(500, "An error occurred while changing the password");
            }

            return NoContent();
        }

        // GET: api/User
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<object>>> GetUsers()
        {
            var users = await _context.Users
                .Include(u => u.Role)
                .Select(u => new
                {
                    u.UserID,
                    u.Username,
                    Role = u.Role.Description,
                    u.CreateAt,
                    u.State,
                    u.Cancellations
                })
                .ToListAsync();

            return users;
        }

        // GET: api/User/customers
        [HttpGet("customers")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<ActionResult<IEnumerable<object>>> GetCustomers()
        {
            var customers = await _context.Customers
                .Include(c => c.User)
                .ThenInclude(u => u.Role)
                .Where(c => c.User.Role.Description == "Customer")
                .Select(c => new
                {
                    c.CustomerID,
                    c.User.Username,
                    c.CustomerName,
                    c.BirthDate,
                    c.IDCard,
                    c.Email,
                    c.PhoneNumber,
                    c.Address,
                    c.User.CreateAt,
                    c.User.State,
                    c.User.Cancellations
                })
                .ToListAsync();

            return customers;
        }

        // GET: api/User/staff
        [HttpGet("staff")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<object>>> GetStaff()
        {
            var staff = await _context.Staffs
                .Include(s => s.User)
                .ThenInclude(u => u.Role)
                .Where(s => s.User.Role.Description == "Staff" || s.User.Role.Description == "Admin")
                .Select(s => new
                {
                    s.StaffID,
                    s.User.Username,
                    Role = s.User.Role.Description,
                    s.StaffName,
                    s.BirthDate,
                    s.IDCard,
                    s.Email,
                    s.PhoneNumber,
                    s.Address,
                    s.User.CreateAt,
                    s.User.State,
                    s.User.Cancellations
                })
                .ToListAsync();

            return staff;
        }

        // POST: api/User/staff
        [HttpPost("staff")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<object>> CreateStaff(StaffRegisterRequest request)
        {
            // Check if username already exists
            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            {
                return BadRequest("Username already exists");
            }

            // Check if email already exists
            if (await _context.Staffs.AnyAsync(s => s.Email == request.Email))
            {
                return BadRequest("Email already exists");
            }

            // Check if phone number already exists
            if (await _context.Staffs.AnyAsync(s => s.PhoneNumber == request.PhoneNumber))
            {
                return BadRequest("Phone number already exists");
            }

            // Check if ID card already exists
            if (await _context.Staffs.AnyAsync(s => s.IDCard == request.IDCard))
            {
                return BadRequest("ID card already exists");
            }

            // Get staff role ID
            var staffRole = await _context.Roles.FirstOrDefaultAsync(r => r.Description == "Staff");
            if (staffRole == null)
            {
                return StatusCode(500, "Staff role not found");
            }

            // Hash password
            var (passwordHash, salt) = _passwordService.HashPassword(request.Password);

            // Create new user
            var user = new User
            {
                Username = request.Username,
                Password = passwordHash,
                Salt = salt,
                RoleID = staffRole.RoleID,
                State = true,
                CreateAt = DateTime.Now
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Create staff profile
            var staff = new Staff
            {
                StaffName = request.StaffName,
                BirthDate = request.BirthDate,
                IDCard = request.IDCard,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                Address = request.Address,
                UserID = user.UserID
            };

            _context.Staffs.Add(staff);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetStaff), new
            {
                staff.StaffID,
                user.Username,
                Role = staffRole.Description,
                staff.StaffName,
                staff.Email
            });
        }

        // PUT: api/User/5/state
        [HttpPut("{id}/state")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ToggleUserState(int id, UserStateRequest request)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound("User not found");
            }

            // Update user state
            user.State = request.State;
            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return StatusCode(500, "An error occurred while updating user state");
            }

            return NoContent();
        }

        // DELETE: api/User/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound("User not found");
            }

            // Check if user is an admin
            var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Description == "Admin");
            if (user.RoleID == adminRole?.RoleID)
            {
                // Count number of admins
                var adminCount = await _context.Users.CountAsync(u => u.RoleID == adminRole.RoleID);
                if (adminCount <= 1)
                {
                    return BadRequest("Cannot delete the last admin user");
                }
            }

            // Check if user has associated customer or staff profile
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserID == id);
            var staff = await _context.Staffs.FirstOrDefaultAsync(s => s.UserID == id);

            // Check if user has associated bookings
            if (customer != null)
            {
                var hasBookings = await _context.Bookings.AnyAsync(b => b.CustomerID == customer.CustomerID);
                if (hasBookings)
                {
                    return BadRequest("Cannot delete user with existing bookings");
                }

                // Remove customer profile
                _context.Customers.Remove(customer);
            }

            if (staff != null)
            {
                var hasAssignedBookings = await _context.Bookings.AnyAsync(b => b.StaffID == staff.StaffID);
                var hasAssignedBills = await _context.Bills.AnyAsync(b => b.StaffID == staff.StaffID);
                var hasAssignedServices = await _context.DetailServices.AnyAsync(ds => ds.StaffID == staff.StaffID);

                if (hasAssignedBookings || hasAssignedBills || hasAssignedServices)
                {
                    return BadRequest("Cannot delete staff with assigned bookings, bills, or services");
                }

                // Remove staff profile
                _context.Staffs.Remove(staff);
            }

            // Remove feedback
            var feedbacks = await _context.Feedbacks.Where(f => f.UserID == id).ToListAsync();
            if (feedbacks.Any())
            {
                _context.Feedbacks.RemoveRange(feedbacks);
            }

            // Remove user
            _context.Users.Remove(user);

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }

    public class ProfileUpdateRequest
    {
        public required string Name { get; set; }
        public DateTime BirthDate { get; set; }
        public required string Email { get; set; }
        public required string PhoneNumber { get; set; }
        public required string Address { get; set; }
    }

    public class PasswordChangeRequest
    {
        public required string CurrentPassword { get; set; }
        public required string NewPassword { get; set; }
    }

    public class StaffRegisterRequest
    {
        public required string Username { get; set; }
        public required string Password { get; set; }
        public required string StaffName { get; set; }
        public DateTime BirthDate { get; set; }
        public required string IDCard { get; set; }
        public required string Email { get; set; }
        public required string PhoneNumber { get; set; }
        public required string Address { get; set; }
    }

    public class UserStateRequest
    {
        public bool State { get; set; }
    }
}
