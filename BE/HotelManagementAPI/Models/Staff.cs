using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelManagementAPI.Models
{
    public class Staff
    {
        [Key]
        public int StaffID { get; set; }
        
        [Required]
        public string StaffName { get; set; }
        
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
        
        [Required]
        public int UserID { get; set; }
        
        // Foreign key relationship
        [ForeignKey("UserID")]
        public virtual User User { get; set; }
        
        // Navigation properties
        public virtual ICollection<Booking> Bookings { get; set; }
        public virtual ICollection<Bill> Bills { get; set; }
        public virtual ICollection<DetailService> DetailServices { get; set; }
    }
}
