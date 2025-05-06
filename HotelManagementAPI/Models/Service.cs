using System.ComponentModel.DataAnnotations;

namespace HotelManagementAPI.Models
{
    public class Service
    {
        [Key]
        public int ServiceID { get; set; }
        
        [Required]
        public string Description { get; set; }
        
        [Required]
        public double Price { get; set; }
        
        // Navigation property
        public virtual ICollection<DetailService> DetailServices { get; set; }
    }
}
