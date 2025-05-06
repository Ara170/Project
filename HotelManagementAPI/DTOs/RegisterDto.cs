using System.ComponentModel.DataAnnotations;

namespace HotelManagementAPI.DTOs
{
    public class RegisterDto
    {
        [Required]
        public string Username { get; set; }
        
        [Required]
        [MinLength(10, ErrorMessage = "Password must be at least 10 characters long")]
        public string Password { get; set; }
        
        [Required]
        public string CustomerName { get; set; }
        
        [Required]
        public DateTime BirthDate { get; set; }
        
        [Required]
        [StringLength(10)]
        public string IDCard { get; set; }
        
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        
        [Required]
        [StringLength(10)]
        public string PhoneNumber { get; set; }
        
        [Required]
        public string Address { get; set; }
    }
}
