using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelManagementAPI.Models
{
    public class Bill
    {
        [Key]
        public int BillsID { get; set; }
        
        [Required]
        public int BookingID { get; set; }
        
        [Required]
        public int DiscountID { get; set; } = 0;
        
        public double? Total { get; set; }
        
        [Required]
        public bool CheckBill { get; set; } = false;
        
        [Required]
        public int StaffID { get; set; }
        
        // Foreign key relationships
        [ForeignKey("BookingID")]
        public virtual Booking? Booking { get; set; }
        
        [ForeignKey("DiscountID")]
        public virtual Discount? Discount { get; set; }
        
        [ForeignKey("StaffID")]
        public virtual Staff? Staff { get; set; }
    }
}
