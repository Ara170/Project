using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelManagementAPI.Data;
using HotelManagementAPI.DTOs;
using HotelManagementAPI.Models;
using HotelManagementAPI.Services;

namespace HotelManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly HotelDbContext _context;
        private readonly IJwtService _jwtService;
        private readonly IPasswordService _passwordService;

        public AuthController(
            HotelDbContext context,
            IJwtService jwtService,
            IPasswordService passwordService)
        {
            _context = context;
            _jwtService = jwtService;
            _passwordService = passwordService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginDto loginDto)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Username == loginDto.Username);

            if (user == null)
            {
                return Unauthorized(new AuthResponseDto
                {
                    Success = false,
                    Message = "Invalid username or password"
                });
            }

            // Verify password
            bool isPasswordValid = _passwordService.VerifyPassword(
                loginDto.Password, 
                user.Password, 
                user.Salt);

            if (!isPasswordValid)
            {
                return Unauthorized(new AuthResponseDto
                {
                    Success = false,
                    Message = "Invalid username or password"
                });
            }

            // Check if account is active
            if (!user.State)
            {
                return Unauthorized(new AuthResponseDto
                {
                    Success = false,
                    Message = "Account is disabled"
                });
            }

            // Generate JWT token
            string roleName = user.Role.Description;
            string token = _jwtService.GenerateJwtToken(user, roleName);

            return Ok(new AuthResponseDto
            {
                Success = true,
                Message = "Login successful",
                Token = token,
                Expiration = DateTime.UtcNow.AddHours(12),
                Username = user.Username,
                Role = roleName
            });
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto registerDto)
        {
            // Check if username already exists
            if (await _context.Users.AnyAsync(u => u.Username == registerDto.Username))
            {
                return BadRequest(new AuthResponseDto
                {
                    Success = false,
                    Message = "Username already exists"
                });
            }

            // Check if email already exists
            if (await _context.Customers.AnyAsync(c => c.Email == registerDto.Email))
            {
                return BadRequest(new AuthResponseDto
                {
                    Success = false,
                    Message = "Email already exists"
                });
            }

            // Check if phone number already exists
            if (await _context.Customers.AnyAsync(c => c.PhoneNumber == registerDto.PhoneNumber))
            {
                return BadRequest(new AuthResponseDto
                {
                    Success = false,
                    Message = "Phone number already exists"
                });
            }

            // Check if ID card already exists
            if (await _context.Customers.AnyAsync(c => c.IDCard == registerDto.IDCard))
            {
                return BadRequest(new AuthResponseDto
                {
                    Success = false,
                    Message = "ID card already exists"
                });
            }

            // Get customer role ID
            var customerRole = await _context.Roles.FirstOrDefaultAsync(r => r.Description == "Customer");
            if (customerRole == null)
            {
                return StatusCode(500, new AuthResponseDto
                {
                    Success = false,
                    Message = "Customer role not found"
                });
            }

            // Hash password
            var (passwordHash, salt) = _passwordService.HashPassword(registerDto.Password);

            // Create new user
            var user = new User
            {
                Username = registerDto.Username,
                Password = passwordHash,
                Salt = salt,
                RoleID = customerRole.RoleID,
                State = true,
                CreateAt = DateTime.Now
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Create customer profile
            var customer = new Customer
            {
                CustomerName = registerDto.CustomerName,
                BirthDate = registerDto.BirthDate,
                IDCard = registerDto.IDCard,
                Email = registerDto.Email,
                PhoneNumber = registerDto.PhoneNumber,
                Address = registerDto.Address,
                UserID = user.UserID
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            // Generate JWT token
            string token = _jwtService.GenerateJwtToken(user, customerRole.Description);

            return Ok(new AuthResponseDto
            {
                Success = true,
                Message = "Registration successful",
                Token = token,
                Expiration = DateTime.UtcNow.AddHours(12),
                Username = user.Username,
                Role = customerRole.Description
            });
        }
    }
}
