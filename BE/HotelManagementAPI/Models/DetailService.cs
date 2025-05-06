using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelManagementAPI.Models
{
    public class DetailService
    {
        [Required]
        public int Booking_Service_ID { get; set; }
        
        [Required]
        public int ServiceID { get; set; }
        
        [Required]
        public double Price { get; set; }
        
        [Required]
        public DateTime DateUse { get; set; }
        
        [Required]
        public int StaffID { get; set; }
        
        // Foreign key relationships
        [ForeignKey("Booking_Service_ID")]
        public virtual DetailBooking DetailBooking { get; set; }
        
        [ForeignKey("ServiceID")]
        public virtual Service Service { get; set; }
        
        [ForeignKey("StaffID")]
        public virtual Staff Staff { get; set; }
    }
}
