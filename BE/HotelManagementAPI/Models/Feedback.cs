using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelManagementAPI.Models
{
    public class Feedback
    {
        [Required]
        public int UserID { get; set; }
        
        public string Comment { get; set; }
        
        [Required]
        public DateTime TimeComment { get; set; } = DateTime.Now;
        
        // Foreign key relationship
        [ForeignKey("UserID")]
        public virtual User User { get; set; }
    }
}
