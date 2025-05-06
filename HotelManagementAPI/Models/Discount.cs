using System.ComponentModel.DataAnnotations;

namespace HotelManagementAPI.Models
{
    public class Discount
    {
        [Key]
        public int DiscountID { get; set; }
        
        [Required]
        public string Description { get; set; }
        
        [Required]
        public double DiscountRoomVlaue { get; set; }
        
        [Required]
        public double DiscountSerciveVlaue { get; set; }
        
        // Navigation property
        public virtual ICollection<Bill> Bills { get; set; }
    }
}
