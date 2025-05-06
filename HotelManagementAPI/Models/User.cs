using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelManagementAPI.Models
{
    public class User
    {
        [Key]
        public int UserID { get; set; }
        
        [Required]
        public string Username { get; set; }
        
        [Required]
        public byte[] Password { get; set; }
        
        [Required]
        public int RoleID { get; set; }
        
        [Required]
        public string Salt { get; set; }
        
        public bool State { get; set; } = true;
        
        public int Cancellations { get; set; } = 0;
        
        public DateTime CreateAt { get; set; } = DateTime.Now;
        
        // Foreign key relationship
        [ForeignKey("RoleID")]
        public virtual Role Role { get; set; }
        
        // Navigation properties
        public virtual Customer Customer { get; set; }
        public virtual Staff Staff { get; set; }
        public virtual ICollection<Feedback> Feedbacks { get; set; }
    }
}
