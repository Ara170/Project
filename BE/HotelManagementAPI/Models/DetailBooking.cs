using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelManagementAPI.Models
{
    public class DetailBooking
    {
        [Key]
        public int Booking_Service_ID { get; set; }
        
        [Required]
        public int BookingID { get; set; }
        
        [Required]
        [StringLength(3)]
        public string RoomID { get; set; }
        
        [Required]
        public double Price { get; set; }
        
        public int? ServiceBookingCount { get; set; }
        
        [Required]
        public DateTime DateIn { get; set; }
        
        [Required]
        public DateTime DateOut { get; set; }
        
        // Foreign key relationships
        [ForeignKey("BookingID")]
        public virtual Booking Booking { get; set; }
        
        [ForeignKey("RoomID")]
        public virtual Room Room { get; set; }
        
        // Navigation property
        public virtual ICollection<DetailService> DetailServices { get; set; }
    }
}
