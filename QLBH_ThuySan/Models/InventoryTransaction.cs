using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLBH_ThuySan.Models
{
    public enum TransactionType
    {
        Purchase = 1,
        Sale = 2,
        Adjustment = 3,
        Transfer = 4
    }

    public class InventoryTransaction
    {
        [Key]
        public int TransactionId { get; set; }

        [Required]
        [StringLength(50)]
        public string TransactionCode { get; set; } = string.Empty;

        public TransactionType TransactionType { get; set; }

        public int ProductId { get; set; }
        public int WarehouseId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        public DateTime TransactionDate { get; set; } = DateTime.Now;

        [StringLength(500)]
        public string? Notes { get; set; }

        public int? PurchaseOrderId { get; set; }
        public int? SalesOrderId { get; set; }

        // Navigation properties
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; } = null!;

        [ForeignKey("WarehouseId")]
        public virtual Warehouse Warehouse { get; set; } = null!;

        [ForeignKey("PurchaseOrderId")]
        public virtual PurchaseOrder? PurchaseOrder { get; set; }

        [ForeignKey("SalesOrderId")]
        public virtual SalesOrder? SalesOrder { get; set; }
    }
}
