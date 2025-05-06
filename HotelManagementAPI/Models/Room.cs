using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelManagementAPI.Models
{
    public class Room
    {
        [Key]
        [StringLength(3)]
        public string RoomID { get; set; }
        
        [Required]
        public int RoomType { get; set; }
        
        [Required]
        public string State { get; set; }
        
        // Foreign key relationship
        [ForeignKey("RoomType")]
        public virtual TypeRoom TypeRoom { get; set; }
        
        // Navigation property
        public virtual ICollection<DetailBooking> DetailBookings { get; set; }
    }
}
