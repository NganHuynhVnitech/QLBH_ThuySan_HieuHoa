using System.ComponentModel.DataAnnotations;

namespace QLBH_ThuySan.Models
{
    public class Supplier
    {
        [Key]
        public int SupplierId { get; set; }

        [Required]
        [StringLength(50)]
        public string SupplierCode { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string SupplierName { get; set; } = string.Empty;

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
        public virtual ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();
        public virtual ICollection<SupplierDiscount> SupplierDiscounts { get; set; } = new List<SupplierDiscount>();
    }
}
