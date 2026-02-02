using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLBH_ThuySan.Models
{
    public enum DiscountType
    {
        Fixed = 1,
        Percentage = 2,
        Tiered = 3
    }

    public class SupplierDiscount
    {
        [Key]
        public int SupplierDiscountId { get; set; }

        public int SupplierId { get; set; }

        [Required]
        [StringLength(100)]
        public string DiscountName { get; set; } = string.Empty;

        public DiscountType DiscountType { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountValue { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal MinimumAmount { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation properties
        [ForeignKey("SupplierId")]
        public virtual Supplier Supplier { get; set; } = null!;

        public virtual ICollection<DiscountTier> DiscountTiers { get; set; } = new List<DiscountTier>();
    }

    public class CustomerDiscount
    {
        [Key]
        public int CustomerDiscountId { get; set; }

        public int CustomerId { get; set; }

        [Required]
        [StringLength(100)]
        public string DiscountName { get; set; } = string.Empty;

        public DiscountType DiscountType { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountValue { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal MinimumAmount { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation properties
        [ForeignKey("CustomerId")]
        public virtual Customer Customer { get; set; } = null!;

        public virtual ICollection<DiscountTier> DiscountTiers { get; set; } = new List<DiscountTier>();
    }

    public class DiscountTier
    {
        [Key]
        public int DiscountTierId { get; set; }

        public int? SupplierDiscountId { get; set; }
        public int? CustomerDiscountId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal FromAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ToAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountValue { get; set; }

        // Navigation properties
        [ForeignKey("SupplierDiscountId")]
        public virtual SupplierDiscount? SupplierDiscount { get; set; }

        [ForeignKey("CustomerDiscountId")]
        public virtual CustomerDiscount? CustomerDiscount { get; set; }
    }
}
