using System.ComponentModel.DataAnnotations;

namespace QLBH_ThuySan.Models
{
    public class Customer
    {
        [Key]
        public int CustomerId { get; set; }

        [Required]
        [StringLength(50)]
        public string CustomerCode { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string CustomerName { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Address { get; set; }

        [StringLength(50)]
        public string? Phone { get; set; }

        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(50)]
        public string? TaxCode { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual ICollection<SalesOrder> SalesOrders { get; set; } = new List<SalesOrder>();
        public virtual ICollection<CustomerDiscount> CustomerDiscounts { get; set; } = new List<CustomerDiscount>();
    }
}
