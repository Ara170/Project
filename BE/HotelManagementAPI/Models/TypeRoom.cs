using System.ComponentModel.DataAnnotations;

namespace HotelManagementAPI.Models
{
    public class TypeRoom
    {
        [Key]
        public int TypeID { get; set; }
        
        [Required]
        public string Description { get; set; }
        
        [Required]
        public double Price { get; set; }
        
        // Navigation property
        public virtual ICollection<Room> Rooms { get; set; }
    }
}
