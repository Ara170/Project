using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelManagementAPI.Models
{
    public class Booking
    {
        [Key]
        public int BookingID { get; set; }
        
        [Required]
        public int CustomerID { get; set; }
        
        public int StaffID { get; set; } = 0;
        
        public int? RoomBookingCount { get; set; }
        
        [Required]
        public DateTime BookingDate { get; set; } = DateTime.Now;
        
        // Foreign key relationships
        [ForeignKey("CustomerID")]
        public virtual Customer Customer { get; set; }
        
        [ForeignKey("StaffID")]
        public virtual Staff Staff { get; set; }
        
        // Navigation properties
        public virtual ICollection<DetailBooking> DetailBookings { get; set; }
        public virtual ICollection<Bill> Bills { get; set; }
    }
}
