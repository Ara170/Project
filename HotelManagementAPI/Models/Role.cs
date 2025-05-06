using System.ComponentModel.DataAnnotations;

namespace HotelManagementAPI.Models
{
    public class Role
    {
        [Key]
        public int RoleID { get; set; }
        
        [Required]
        public string Description { get; set; }
        
        [Required]
        public int Level { get; set; }
        
        // Navigation property
        public virtual ICollection<User> Users { get; set; }
    }
}
